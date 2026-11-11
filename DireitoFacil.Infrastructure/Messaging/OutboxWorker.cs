using System.Text.Json;
using System.Text.Json.Nodes;
using DireitoFacil.Application.Abstractions;
using DireitoFacil.Application.Events;
using DireitoFacil.Infrastructure.Persistence;
using DireitoFacil.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DireitoFacil.Infrastructure.Messaging;

/// <summary>
/// Varre a tabela DF_OUTBOX e publica no RabbitMQ os eventos ainda não processados.
/// Se o RabbitMQ estiver fora do ar, os eventos ficam na tabela e são tentados de novo
/// na próxima varredura — nenhum laudo se perde.
/// </summary>
public sealed class OutboxWorker(
    IServiceScopeFactory scopeFactory,
    RabbitMqPublisher publisher,
    IOptions<RabbitMqOptions> options,
    ILogger<OutboxWorker> logger) : BackgroundService
{
    private const int TamanhoLote = 20;
    private readonly RabbitMqOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Configurado)
        {
            logger.LogWarning("RabbitMq:Uri não configurada: eventos ficarão acumulados no outbox.");
            return;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(Math.Max(1, _options.IntervaloSegundos)));
        do
        {
            try
            {
                await ProcessarLoteAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Falha ao processar o outbox");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ProcessarLoteAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DireitoFacilContext>();
        var storage = scope.ServiceProvider.GetRequiredService<IArquivoStorage>();

        var pendentes = await context.OutboxMessages
            .Where(m => m.ProcessadoEm == null && m.Tentativas < OutboxMessage.MaxTentativas)
            .OrderBy(m => m.CriadoEm)
            .Take(TamanhoLote)
            .ToListAsync(cancellationToken);

        foreach (var mensagem in pendentes)
        {
            try
            {
                var payload = PrepararPayload(mensagem, storage);
                await publisher.PublicarAsync(
                    _options.FilaDocumentosRecebidos, mensagem.Id.ToString(), payload, cancellationToken);

                mensagem.MarcarProcessado();
                logger.LogInformation("Evento {Tipo} {MensagemId} publicado", mensagem.Tipo, mensagem.Id);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                mensagem.RegistrarFalha(ex.Message);
                logger.LogWarning(ex, "Falha ao publicar {MensagemId} (tentativa {Tentativa})", mensagem.Id, mensagem.Tentativas);
            }
        }

        if (pendentes.Count > 0)
            await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Para DocumentoRecebido, acrescenta uma URL temporária de leitura do arquivo,
    /// gerada só agora para não expirar enquanto o evento esperava na fila.
    /// </summary>
    private string PrepararPayload(OutboxMessage mensagem, IArquivoStorage storage)
    {
        if (mensagem.Tipo != nameof(DocumentoRecebidoEvent))
            return mensagem.Payload;

        var json = JsonNode.Parse(mensagem.Payload)!.AsObject();
        var blobPath = json["blobPath"]?.GetValue<string>();

        if (!string.IsNullOrWhiteSpace(blobPath))
        {
            var url = storage.GerarUrlLeitura(blobPath, TimeSpan.FromMinutes(_options.ValidadeUrlMinutos));
            json["arquivoUrl"] = url.ToString();
        }

        return json.ToJsonString(OutboxEventPublisher.JsonOptions);
    }
}
