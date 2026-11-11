using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DireitoFacil.Application.Abstractions;
using DireitoFacil.Infrastructure.Persistence;
using DireitoFacil.Infrastructure.Persistence.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DireitoFacil.Api.Filters;

/// <summary>
/// Implementa idempotência com o header Idempotency-Key (UUID gerado pelo app a cada ação):
/// <list type="bullet">
/// <item>chave nova: executa a action e guarda status + corpo da resposta;</item>
/// <item>mesma chave e mesmo conteúdo: devolve a resposta guardada, sem executar de novo (header Idempotent-Replayed);</item>
/// <item>mesma chave com conteúdo diferente: 422;</item>
/// <item>mesma chave ainda em processamento (requisições simultâneas): 409.</item>
/// </list>
/// A chave é única por usuário + endpoint e expira em 24 horas.
/// </summary>
public sealed class IdempotenciaFilter(
    DireitoFacilContext context,
    IUsuarioAtual usuarioAtual,
    IOptions<JsonOptions> jsonOptions,
    ILogger<IdempotenciaFilter> logger) : IAsyncActionFilter
{
    public const string Header = "Idempotency-Key";
    public const string HeaderReplay = "Idempotent-Replayed";
    private static readonly TimeSpan Validade = TimeSpan.FromHours(24);

    public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        var http = ctx.HttpContext;
        var cancellationToken = http.RequestAborted;

        if (!Guid.TryParse(http.Request.Headers[Header].ToString(), out var chave) || chave == Guid.Empty)
        {
            ctx.Result = Problema(StatusCodes.Status400BadRequest, "Idempotency-Key obrigatório",
                $"Envie o header {Header} com um UUID novo para cada ação (e o mesmo UUID ao reenviar).");
            return;
        }

        var usuarioId = usuarioAtual.Id;
        var endpoint = $"{http.Request.Method} /{ctx.ActionDescriptor.AttributeRouteInfo?.Template ?? http.Request.Path.Value}";
        var fingerprint = await CalcularFingerprintAsync(endpoint, ctx.ActionArguments, cancellationToken);

        var existente = await Registros(chave, usuarioId, endpoint).AsNoTracking().FirstOrDefaultAsync(cancellationToken);

        if (existente is not null && existente.EstaExpirado(DateTime.UtcNow))
        {
            await Registros(chave, usuarioId, endpoint).ExecuteDeleteAsync(cancellationToken);
            existente = null;
        }

        if (existente is not null)
        {
            if (existente.Fingerprint != fingerprint)
            {
                ctx.Result = Problema(StatusCodes.Status422UnprocessableEntity, "Idempotency-Key reutilizado",
                    "Esta chave já foi usada com outro conteúdo. Gere um novo UUID para uma nova ação.");
                return;
            }

            if (!existente.EstaConcluido)
            {
                ctx.Result = Problema(StatusCodes.Status409Conflict, "Requisição em processamento",
                    "A requisição original com esta chave ainda está sendo processada. Tente novamente em instantes.");
                return;
            }

            logger.LogInformation("Requisição idempotente repetida em {Endpoint}: devolvendo resposta guardada", endpoint);
            http.Response.Headers[HeaderReplay] = "true";
            ctx.Result = existente.RespostaJson is null
                ? new StatusCodeResult(existente.StatusCode!.Value)
                : new ContentResult
                {
                    StatusCode = existente.StatusCode,
                    Content = existente.RespostaJson,
                    ContentType = "application/json; charset=utf-8"
                };
            return;
        }

        // Reserva a chave antes de executar: duas requisições simultâneas não passam juntas (PK no banco).
        var registro = new IdempotencyRecord(chave, usuarioId, endpoint, fingerprint, Validade);
        context.IdempotencyRecords.Add(registro);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            context.Entry(registro).State = EntityState.Detached;
            ctx.Result = Problema(StatusCodes.Status409Conflict, "Requisição em processamento",
                "Outra requisição com esta chave está sendo processada.");
            return;
        }

        var executado = await next();

        // ExecuteUpdate/ExecuteDelete vão direto ao banco, sem salvar nada que a action
        // tenha deixado pendente no contexto em caso de erro.
        var status = ObterStatus(executado);
        if (executado.Exception is not null || status >= 500)
        {
            // Erro: libera a chave para o app poder tentar de novo.
            await Registros(chave, usuarioId, endpoint).ExecuteDeleteAsync(CancellationToken.None);
            return;
        }

        var corpo = ObterCorpoJson(executado);
        await Registros(chave, usuarioId, endpoint).ExecuteUpdateAsync(s => s
            .SetProperty(r => r.StatusCode, status)
            .SetProperty(r => r.RespostaJson, corpo), CancellationToken.None);
    }

    private IQueryable<IdempotencyRecord> Registros(Guid chave, string usuarioId, string endpoint)
        => context.IdempotencyRecords.Where(r => r.Chave == chave && r.UsuarioId == usuarioId && r.Endpoint == endpoint);

    private static int ObterStatus(ActionExecutedContext executado) => executado.Result switch
    {
        ObjectResult o => o.StatusCode ?? StatusCodes.Status200OK,
        IStatusCodeActionResult s => s.StatusCode ?? StatusCodes.Status200OK,
        _ => StatusCodes.Status200OK
    };

    private string? ObterCorpoJson(ActionExecutedContext executado)
        => executado.Result is ObjectResult { Value: not null } o
            ? JsonSerializer.Serialize(o.Value, o.Value.GetType(), jsonOptions.Value.JsonSerializerOptions)
            : null;

    /// <summary>
    /// "Impressão digital" da requisição: endpoint + argumentos da action. Arquivos entram
    /// pelo SHA-256 do conteúdo, então reenviar o mesmo arquivo gera o mesmo fingerprint.
    /// </summary>
    private static async Task<string> CalcularFingerprintAsync(
        string endpoint, IDictionary<string, object?> argumentos, CancellationToken cancellationToken)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hash.AppendData(Encoding.UTF8.GetBytes(endpoint));

        foreach (var (nome, valor) in argumentos.OrderBy(a => a.Key, StringComparer.Ordinal))
        {
            if (valor is CancellationToken)
                continue;

            hash.AppendData(Encoding.UTF8.GetBytes($"|{nome}="));
            await AdicionarValorAsync(hash, valor, cancellationToken);
        }

        return Convert.ToHexString(hash.GetHashAndReset());
    }

    private static async Task AdicionarValorAsync(IncrementalHash hash, object? valor, CancellationToken cancellationToken)
    {
        switch (valor)
        {
            case null:
                hash.AppendData("null"u8);
                break;
            case IFormFile arquivo:
                await using (var stream = arquivo.OpenReadStream())
                    hash.AppendData(await SHA256.HashDataAsync(stream, cancellationToken));
                break;
            case string or Guid or Enum or bool or int or long or decimal or double or DateTime:
                hash.AppendData(Encoding.UTF8.GetBytes(Convert.ToString(valor, System.Globalization.CultureInfo.InvariantCulture)!));
                break;
            default:
                // Objetos (DTOs e formulários): propriedade por propriedade, tratando IFormFile pelo conteúdo.
                foreach (var propriedade in valor.GetType().GetProperties().OrderBy(p => p.Name, StringComparer.Ordinal))
                {
                    if (propriedade.GetIndexParameters().Length > 0)
                        continue;

                    hash.AppendData(Encoding.UTF8.GetBytes($";{propriedade.Name}:"));
                    var valorPropriedade = propriedade.GetValue(valor);
                    if (valorPropriedade is IFormFile or null)
                        await AdicionarValorAsync(hash, valorPropriedade, cancellationToken);
                    else
                        hash.AppendData(JsonSerializer.SerializeToUtf8Bytes(valorPropriedade, valorPropriedade.GetType()));
                }
                break;
        }
    }

    private static ObjectResult Problema(int status, string titulo, string detalhe) => new(new ProblemDetails
    {
        Type = "about:blank",
        Title = titulo,
        Status = status,
        Detail = detalhe
    })
    {
        StatusCode = status,
        ContentTypes = { "application/problem+json" }
    };
}
