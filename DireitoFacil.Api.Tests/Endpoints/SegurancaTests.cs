using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DireitoFacil.Api.Tests.Infra;
using DiretoFacil.Domain.Enums;

namespace DireitoFacil.Api.Tests.Endpoints;

/// <summary>
/// JWT e controle de acesso por papel (CIDADAO, GESTOR, SERVICO).
/// </summary>
public class SegurancaTests(DireitoFacilApiFactory factory) : IClassFixture<DireitoFacilApiFactory>
{
    [Theory]
    [InlineData("/api/documentos")]
    [InlineData("/api/envios")]
    public async Task SemToken_DeveRetornar401(string url)
    {
        // Act
        var resposta = await factory.CreateClient().GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task TokenAdulterado_DeveRetornar401()
    {
        // Arrange: token assinado com outra chave
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJoYWNrZXIiLCJyb2xlIjoiR0VTVE9SIn0.assinatura-falsa");

        // Act
        var resposta = await client.GetAsync("/api/envios");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Gestor_TentandoEnviarDocumento_DeveRetornar403()
    {
        // Arrange
        var gestor = await factory.CriarClienteAsync(PapelUsuarioEnum.Gestor);

        // Act
        var resposta = await gestor.Client.EnviarDocumentoAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task Cidadao_TentandoAnalisarEnvio_DeveRetornar403()
    {
        // Arrange
        var cidadao = await factory.CriarClienteAsync();

        // Act
        var resposta = await cidadao.Client.PostJsonAsync(
            $"/api/envios/{Guid.NewGuid()}/analise", new { decisao = "Aprovar" }, semChave: true);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task Cidadao_TentandoAtualizarStatusDaIa_DeveRetornar403()
    {
        // Arrange
        var cidadao = await factory.CriarClienteAsync();
        var documento = await cidadao.Client.CriarDocumentoAsync();

        // Act
        var resposta = await cidadao.Client.PatchAsJsonAsync(
            $"/api/documentos/{documento.Id}/status", new { status = "Extraido" }, ApiHelpers.Json);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task Servico_AtualizandoStatusDaIa_DeveRetornar204()
    {
        // Arrange
        var cidadao = await factory.CriarClienteAsync();
        var documento = await cidadao.Client.CriarDocumentoAsync();
        var servico = await factory.CriarClienteAsync(PapelUsuarioEnum.Servico);

        // Act
        var resposta = await servico.Client.PatchAsJsonAsync(
            $"/api/documentos/{documento.Id}/status", new { status = "Extraido" }, ApiHelpers.Json);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        var status = await factory.NoBancoAsync(async db => (await db.Documentos.FindAsync(documento.Id))!.Status);
        Assert.Equal(StatusDocumentoEnum.Extraido, status);
    }
}
