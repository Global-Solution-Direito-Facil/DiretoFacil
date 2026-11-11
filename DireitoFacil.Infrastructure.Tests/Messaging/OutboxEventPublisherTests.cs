using System.Text.Json;
using DireitoFacil.Application.Events;
using DireitoFacil.Infrastructure.Messaging;
using DireitoFacil.Infrastructure.Tests.Infra;
using Microsoft.EntityFrameworkCore;

namespace DireitoFacil.Infrastructure.Tests.Messaging;

public class OutboxEventPublisherTests : IDisposable
{
    private readonly BancoSqlite _banco = new();

    public void Dispose() => _banco.Dispose();

    private static DocumentoRecebidoEvent Evento(Guid documentoId)
        => new(Guid.NewGuid(), documentoId, BancoSqlite.CidadaoA, "Laudo", "application/pdf", "cidadao/arquivo", DateTime.UtcNow);

    [Fact]
    public async Task Publicar_NaoDeveGravarAntesDoSaveChanges()
    {
        // Arrange
        await using var contexto = _banco.NovoContexto();
        var publisher = new OutboxEventPublisher(contexto);

        // Act
        publisher.Publicar(Evento(Guid.NewGuid()));

        // Assert: o evento só existe no rastreador do contexto, não no banco
        await using var verificacao = _banco.NovoContexto();
        Assert.Equal(0, await verificacao.OutboxMessages.CountAsync());
    }

    [Fact]
    public async Task Publicar_ComSaveChanges_DeveGravarEventoJuntoComODocumento()
    {
        // Arrange
        var documento = BancoSqlite.Documento();
        await using (var contexto = _banco.NovoContexto())
        {
            contexto.Documentos.Add(documento);
            new OutboxEventPublisher(contexto).Publicar(Evento(documento.Id));

            // Act: uma única transação para os dois
            await contexto.SaveChangesAsync();
        }

        // Assert
        await using var verificacao = _banco.NovoContexto();
        var mensagem = await verificacao.OutboxMessages.SingleAsync();
        Assert.Equal(nameof(DocumentoRecebidoEvent), mensagem.Tipo);
        Assert.Null(mensagem.ProcessadoEm);
        Assert.True(await verificacao.Documentos.AnyAsync(d => d.Id == documento.Id));

        // Payload em camelCase, no formato combinado com a API Java
        using var json = JsonDocument.Parse(mensagem.Payload);
        Assert.Equal(documento.Id, json.RootElement.GetProperty("documentoId").GetGuid());
        Assert.Equal("Laudo", json.RootElement.GetProperty("tipoDocumento").GetString());
    }

    [Fact]
    public async Task Publicar_SeOSaveChangesFalhar_NaoDeveGravarNemDocumentoNemEvento()
    {
        // Arrange: dois documentos com o mesmo hash → o índice único faz o SaveChanges falhar
        var documento = BancoSqlite.Documento(hash: "DUPLICADO");
        var duplicado = BancoSqlite.Documento(hash: "DUPLICADO");

        await using (var contexto = _banco.NovoContexto())
        {
            contexto.Documentos.AddRange(documento, duplicado);
            new OutboxEventPublisher(contexto).Publicar(Evento(documento.Id));

            // Act
            await Assert.ThrowsAsync<DbUpdateException>(() => contexto.SaveChangesAsync());
        }

        // Assert: nada pela metade
        await using var verificacao = _banco.NovoContexto();
        Assert.Equal(0, await verificacao.OutboxMessages.CountAsync());
        Assert.Equal(0, await verificacao.Documentos.CountAsync());
    }
}
