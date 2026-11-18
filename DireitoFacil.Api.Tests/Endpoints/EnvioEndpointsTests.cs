using System.Net;
using System.Net.Http.Json;
using DireitoFacil.Api.Tests.Infra;
using DireitoFacil.Application.DTOs;
using DireitoFacil.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DireitoFacil.Api.Tests.Endpoints;

/// <summary>
/// Fluxo principal: cidadão monta e submete o envio, assistente social devolve e aprova.
/// </summary>
public class EnvioEndpointsTests(DireitoFacilApiFactory factory) : IClassFixture<DireitoFacilApiFactory>
{
    [Fact]
    public async Task FluxoCompleto_RascunhoSubmeterDevolverCorrigirAprovar()
    {
        // Arrange: os 3 documentos que a CIPTEA exige no catálogo
        var maria = await factory.CriarClienteAsync();
        var rg = await maria.Client.CriarDocumentoAsync(TipoDocumentoEnum.Rg);
        var cpf = await maria.Client.CriarDocumentoAsync(TipoDocumentoEnum.Cpf);
        var laudo = await maria.Client.CriarDocumentoAsync(TipoDocumentoEnum.Laudo);
        var gestor = await factory.CriarClienteAsync(PapelUsuarioEnum.Gestor);

        // 1) Cria só com o RG
        var criar = await maria.Client.PostJsonAsync("/api/envios", new CriarEnvioRequest(7, "ciptea", [rg.Id]));
        Assert.Equal(HttpStatusCode.Created, criar.StatusCode);
        var envio = await criar.LerAsync<EnvioResponse>();
        Assert.Equal(StatusEnvioEnum.Rascunho, envio.Status);
        Assert.Equal("CIPTEA", envio.DireitoCodigo);

        // 2) Submeter falha: faltam CPF e Laudo
        var faltando = await maria.Client.PostJsonAsync($"/api/envios/{envio.Id}/submeter");
        Assert.Equal(HttpStatusCode.Conflict, faltando.StatusCode);

        // 3) Completa os documentos e submete
        var atualizar = await maria.Client.PutAsJsonAsync($"/api/envios/{envio.Id}",
            new AtualizarEnvioRequest([rg.Id, cpf.Id, laudo.Id]), ApiHelpers.Json);
        Assert.Equal(HttpStatusCode.OK, atualizar.StatusCode);

        var submeter = await maria.Client.PostJsonAsync($"/api/envios/{envio.Id}/submeter");
        Assert.Equal(HttpStatusCode.OK, submeter.StatusCode);
        Assert.Equal(StatusEnvioEnum.Enviado, (await submeter.LerAsync<EnvioResponse>()).Status);

        // 4) Gestor: devolver sem observação é inválido; com observação, devolve
        var semObservacao = await gestor.Client.PostJsonAsync($"/api/envios/{envio.Id}/analise",
            new AnaliseEnvioRequest(DecisaoAnaliseEnum.Devolver), semChave: true);
        Assert.Equal(HttpStatusCode.BadRequest, semObservacao.StatusCode);

        var devolver = await gestor.Client.PostJsonAsync($"/api/envios/{envio.Id}/analise",
            new AnaliseEnvioRequest(DecisaoAnaliseEnum.Devolver, "Foto do RG ilegível"), semChave: true);
        Assert.Equal(HttpStatusCode.OK, devolver.StatusCode);
        Assert.Equal(StatusEnvioEnum.Devolvido, (await devolver.LerAsync<EnvioResponse>()).Status);

        // 5) Cidadão reenvia e o gestor aprova
        var reenviar = await maria.Client.PostJsonAsync($"/api/envios/{envio.Id}/submeter");
        Assert.Equal(HttpStatusCode.OK, reenviar.StatusCode);

        var aprovar = await gestor.Client.PostJsonAsync($"/api/envios/{envio.Id}/analise",
            new AnaliseEnvioRequest(DecisaoAnaliseEnum.Aprovar), semChave: true);
        Assert.Equal(HttpStatusCode.OK, aprovar.StatusCode);

        // Assert final: conferido direto no banco
        var gravado = await factory.NoBancoAsync(db => db.Envios.SingleAsync(e => e.Id == envio.Id));
        Assert.Equal(StatusEnvioEnum.Aprovado, gravado.Status);
        Assert.Equal(gestor.Usuario.Email, gravado.GestorId);
    }

    [Fact]
    public async Task Criar_MesmaChaveDuasVezes_DeveCriarUmEnvioSo()
    {
        // Arrange
        var maria = await factory.CriarClienteAsync();
        var rg = await maria.Client.CriarDocumentoAsync();
        var chave = Guid.NewGuid();
        var corpo = new CriarEnvioRequest(1, "CIPTEA", [rg.Id]);

        // Act
        var primeira = await maria.Client.PostJsonAsync("/api/envios", corpo, chave);
        var segunda = await maria.Client.PostJsonAsync("/api/envios", corpo, chave);

        // Assert
        Assert.Equal(HttpStatusCode.Created, segunda.StatusCode);
        Assert.True(segunda.Headers.Contains(ApiHelpers.HeaderReplay));
        Assert.Equal((await primeira.LerAsync<EnvioResponse>()).Id, (await segunda.LerAsync<EnvioResponse>()).Id);

        var quantidade = await factory.NoBancoAsync(db => db.Envios.CountAsync(e => e.CidadaoId == maria.Usuario.Email));
        Assert.Equal(1, quantidade);
    }

    [Fact]
    public async Task Criar_DireitoForaDoCatalogo_DeveRetornar400()
    {
        // Arrange
        var maria = await factory.CriarClienteAsync();
        var rg = await maria.Client.CriarDocumentoAsync();

        // Act
        var resposta = await maria.Client.PostJsonAsync("/api/envios", new CriarEnvioRequest(1, "NAO_EXISTE", [rg.Id]));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Criar_ComDocumentoDeOutroCidadao_DeveRetornar404()
    {
        // Arrange
        var maria = await factory.CriarClienteAsync();
        var joao = await factory.CriarClienteAsync();
        var documentoDoJoao = await joao.Client.CriarDocumentoAsync();

        // Act
        var resposta = await maria.Client.PostJsonAsync("/api/envios",
            new CriarEnvioRequest(1, "CIPTEA", [documentoDoJoao.Id]));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Criar_SemDocumentos_DeveRetornar400()
    {
        // Arrange
        var maria = await factory.CriarClienteAsync();

        // Act
        var resposta = await maria.Client.PostJsonAsync("/api/envios", new CriarEnvioRequest(1, "CIPTEA", []));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Listar_Gestor_DeveVerAFilaDeEnviadosDeTodosOsCidadaos()
    {
        // Arrange: Ana e Bia submetem; Bia ainda tem outro envio em rascunho
        var ana = await factory.CriarClienteAsync();
        var bia = await factory.CriarClienteAsync();
        var gestor = await factory.CriarClienteAsync(PapelUsuarioEnum.Gestor);

        var enviadoAna = await EnviarCompletoAsync(ana);
        var enviadoBia = await EnviarCompletoAsync(bia);

        var rgBia = await bia.Client.CriarDocumentoAsync();
        var rascunho = await (await bia.Client.PostJsonAsync("/api/envios",
            new CriarEnvioRequest(1, "CIPTEA", [rgBia.Id]))).LerAsync<EnvioResponse>();

        // Act
        var resposta = await gestor.Client.GetAsync("/api/envios?status=Enviado&pageSize=100");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var fila = await resposta.LerAsync<PagedResponse<EnvioResponse>>();
        Assert.All(fila.Items, e => Assert.Equal(StatusEnvioEnum.Enviado, e.Status));
        Assert.Contains(fila.Items, e => e.Id == enviadoAna);
        Assert.Contains(fila.Items, e => e.Id == enviadoBia);
        Assert.DoesNotContain(fila.Items, e => e.Id == rascunho.Id);
    }

    /// <summary>Cria e submete um envio de TARIFA_SOCIAL_ENERGIA com os documentos exigidos (CPF + comprovante de residência).</summary>
    private static async Task<Guid> EnviarCompletoAsync(ClienteAutenticado cidadao)
    {
        var cpf = await cidadao.Client.CriarDocumentoAsync(TipoDocumentoEnum.Cpf);
        var residencia = await cidadao.Client.CriarDocumentoAsync(TipoDocumentoEnum.ComprovanteResidencia);

        var envio = await (await cidadao.Client.PostJsonAsync("/api/envios",
            new CriarEnvioRequest(1, "TARIFA_SOCIAL_ENERGIA", [cpf.Id, residencia.Id]))).LerAsync<EnvioResponse>();

        var submeter = await cidadao.Client.PostJsonAsync($"/api/envios/{envio.Id}/submeter");
        Assert.Equal(HttpStatusCode.OK, submeter.StatusCode);
        return envio.Id;
    }
}
