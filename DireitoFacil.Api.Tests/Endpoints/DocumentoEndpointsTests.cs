using System.Net;
using System.Text;
using DireitoFacil.Api.Tests.Infra;
using DireitoFacil.Application.DTOs;
using DireitoFacil.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DireitoFacil.Api.Tests.Endpoints;

/// <summary>
/// Upload, idempotência, validações, isolamento entre cidadãos e exclusão.
/// Cada teste usa um cidadão novo (o rate limit de upload é por usuário).
/// </summary>
public class DocumentoEndpointsTests(DireitoFacilApiFactory factory) : IClassFixture<DireitoFacilApiFactory>
{
    [Fact]
    public async Task Enviar_PdfValido_DeveRetornar201ComLocation()
    {
        // Arrange
        var cidadao = await factory.CriarClienteAsync();

        // Act
        var resposta = await cidadao.Client.EnviarDocumentoAsync(TipoDocumentoEnum.Rg);

        // Assert
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.NotNull(resposta.Headers.Location);
        var documento = await resposta.LerAsync<DocumentoResponse>();
        Assert.Equal(StatusDocumentoEnum.Recebido, documento.Status);
        Assert.Equal(cidadao.Usuario.Email, documento.CidadaoId);
    }

    [Fact]
    public async Task Enviar_MesmaChaveMesmoArquivo_DeveDevolverAMesmaRespostaSemDuplicar()
    {
        // Arrange
        var cidadao = await factory.CriarClienteAsync();
        var chave = Guid.NewGuid();
        var pdf = ApiHelpers.Pdf();

        // Act: o app reenvia a mesma requisição (ex.: a internet caiu)
        var primeira = await cidadao.Client.EnviarDocumentoAsync(conteudo: pdf, chave: chave);
        var segunda = await cidadao.Client.EnviarDocumentoAsync(conteudo: pdf, chave: chave);

        // Assert
        Assert.Equal(HttpStatusCode.Created, primeira.StatusCode);
        Assert.Equal(HttpStatusCode.Created, segunda.StatusCode);
        Assert.False(primeira.Headers.Contains(ApiHelpers.HeaderReplay));
        Assert.True(segunda.Headers.Contains(ApiHelpers.HeaderReplay));

        var doc1 = await primeira.LerAsync<DocumentoResponse>();
        var doc2 = await segunda.LerAsync<DocumentoResponse>();
        Assert.Equal(doc1.Id, doc2.Id);

        var quantidade = await factory.NoBancoAsync(db =>
            db.Documentos.CountAsync(d => d.CidadaoId == cidadao.Usuario.Email));
        Assert.Equal(1, quantidade);
    }

    [Fact]
    public async Task Enviar_MesmaChaveOutroArquivo_DeveRetornar422()
    {
        // Arrange
        var cidadao = await factory.CriarClienteAsync();
        var chave = Guid.NewGuid();
        await cidadao.Client.EnviarDocumentoAsync(conteudo: ApiHelpers.Pdf("a"), chave: chave);

        // Act
        var resposta = await cidadao.Client.EnviarDocumentoAsync(conteudo: ApiHelpers.Pdf("b"), chave: chave);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task Enviar_SemIdempotencyKey_DeveRetornar400()
    {
        // Arrange
        var cidadao = await factory.CriarClienteAsync();

        // Act
        var resposta = await cidadao.Client.EnviarDocumentoAsync(semChave: true);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Enviar_MesmoArquivoComChaveNova_DeveRetornar409()
    {
        // Arrange
        var cidadao = await factory.CriarClienteAsync();
        var pdf = ApiHelpers.Pdf();
        await cidadao.Client.EnviarDocumentoAsync(conteudo: pdf);

        // Act: outra ação, mas o mesmo arquivo (hash igual)
        var resposta = await cidadao.Client.EnviarDocumentoAsync(conteudo: pdf);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task Enviar_LaudoSemConsentimento_DeveRetornar400()
    {
        // Arrange
        var cidadao = await factory.CriarClienteAsync();

        // Act
        var resposta = await cidadao.Client.EnviarDocumentoAsync(TipoDocumentoEnum.Laudo, consentimentoSaude: false);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Enviar_ArquivoQueNaoEPdf_DeveRetornar400()
    {
        // Arrange: texto comum declarado como PDF
        var cidadao = await factory.CriarClienteAsync();

        // Act
        var resposta = await cidadao.Client.EnviarDocumentoAsync(conteudo: Encoding.UTF8.GetBytes("não sou um pdf"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Obter_DocumentoDeOutroCidadao_DeveRetornar404()
    {
        // Arrange
        var dona = await factory.CriarClienteAsync();
        var documento = await dona.Client.CriarDocumentoAsync();
        var outro = await factory.CriarClienteAsync();

        // Act
        var resposta = await outro.Client.GetAsync($"/api/documentos/{documento.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Listar_DeveRetornarSoOsDocumentosDoCidadao()
    {
        // Arrange
        var maria = await factory.CriarClienteAsync();
        await maria.Client.CriarDocumentoAsync(TipoDocumentoEnum.Rg);
        await maria.Client.CriarDocumentoAsync(TipoDocumentoEnum.Cpf);
        var joao = await factory.CriarClienteAsync();
        await joao.Client.CriarDocumentoAsync(TipoDocumentoEnum.Rg);

        // Act: mesmo tentando filtrar pelo João, a Maria só vê os dela
        var resposta = await maria.Client.GetAsync($"/api/documentos?cidadaoId={joao.Usuario.Email}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var pagina = await resposta.LerAsync<PagedResponse<DocumentoResponse>>();
        Assert.Equal(2, pagina.TotalItems);
        Assert.All(pagina.Items, d => Assert.Equal(maria.Usuario.Email, d.CidadaoId));
    }

    [Fact]
    public async Task Listar_PageSizeAcimaDoLimite_DeveRetornar400()
    {
        // Arrange
        var cidadao = await factory.CriarClienteAsync();

        // Act
        var resposta = await cidadao.Client.GetAsync("/api/documentos?pageSize=500");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task BaixarArquivo_DeveDevolverOMesmoConteudoEnviado()
    {
        // Arrange
        var cidadao = await factory.CriarClienteAsync();
        var pdf = ApiHelpers.Pdf();
        var criado = await (await cidadao.Client.EnviarDocumentoAsync(conteudo: pdf)).LerAsync<DocumentoResponse>();

        // Act
        var resposta = await cidadao.Client.GetAsync($"/api/documentos/{criado.Id}/arquivo");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal(pdf, await resposta.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task Excluir_DocumentoLivre_DeveRetornar204()
    {
        // Arrange
        var cidadao = await factory.CriarClienteAsync();
        var documento = await cidadao.Client.CriarDocumentoAsync();

        // Act
        var resposta = await cidadao.Client.DeleteAsync($"/api/documentos/{documento.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await cidadao.Client.GetAsync($"/api/documentos/{documento.Id}")).StatusCode);
    }

    [Fact]
    public async Task Excluir_DocumentoEmEnvio_DeveRetornar409()
    {
        // Arrange
        var cidadao = await factory.CriarClienteAsync();
        var documento = await cidadao.Client.CriarDocumentoAsync();
        var envio = await cidadao.Client.PostJsonAsync("/api/envios",
            new CriarEnvioRequest(1, "CIPTEA", [documento.Id]));
        Assert.Equal(HttpStatusCode.Created, envio.StatusCode);

        // Act
        var resposta = await cidadao.Client.DeleteAsync($"/api/documentos/{documento.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }
}
