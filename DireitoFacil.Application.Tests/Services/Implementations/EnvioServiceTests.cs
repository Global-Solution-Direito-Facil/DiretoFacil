using DireitoFacil.Application.DTOs;
using DireitoFacil.Application.Services.Implementations;
using DireitoFacil.Application.Tests.Fakes;
using DiretoFacil.Domain.Entites;
using DiretoFacil.Domain.Enums;
using DiretoFacil.Domain.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;

namespace DireitoFacil.Application.Tests.Services.Implementations;

public class EnvioServiceTests
{
    private readonly BancoEmMemoria _banco = new();

    public EnvioServiceTests()
    {
        // Catálogo usado nos testes: CIPTEA exige apenas RG (os documentos de teste são RG).
        _banco.Obrigatorios.Add(new DocumentoObrigatorio("CIPTEA", TipoDocumentoEnum.Rg));
        _banco.Obrigatorios.Add(new DocumentoObrigatorio("BPC_PCD", TipoDocumentoEnum.Rg));
        _banco.Obrigatorios.Add(new DocumentoObrigatorio("BPC_PCD", TipoDocumentoEnum.Laudo));
    }

    private EnvioService CriarService(FakeUsuarioAtual? usuario = null) => new(
        new FakeEnvioRepository(_banco),
        new FakeDocumentoRepository(_banco),
        new FakeDocumentoObrigatorioRepository(_banco),
        _banco,
        usuario ?? FakeUsuarioAtual.Cidadao(),
        NullLogger<EnvioService>.Instance);

    private Documento NovoDocumento(string cidadaoId = Dados.CidadaoA)
    {
        var documento = Dados.Documento(cidadaoId);
        _banco.Documentos.Add(documento);
        return documento;
    }

    private Envio NovoEnvio(string cidadaoId = Dados.CidadaoA)
    {
        var envio = new Envio(cidadaoId, 1, "CIPTEA", [NovoDocumento(cidadaoId)]);
        _banco.Envios.Add(envio);
        return envio;
    }

    [Fact]
    public async Task CriarAsync_ComDocumentosDoCidadao_DeveCriarRascunho()
    {
        // Arrange
        var documento = NovoDocumento();
        var service = CriarService();

        // Act
        var response = await service.CriarAsync(new CriarEnvioRequest(7, "ciptea", [documento.Id]));

        // Assert
        Assert.Equal(StatusEnvioEnum.Rascunho, response.Status);
        Assert.Equal("CIPTEA", response.DireitoCodigo);
        Assert.Equal(documento.Id, Assert.Single(response.DocumentoIds));
        Assert.Single(_banco.Envios);
        Assert.Equal(1, _banco.Salvamentos);
    }

    [Fact]
    public async Task CriarAsync_ComDocumentoDeOutroCidadao_DeveLancarKeyNotFoundException()
    {
        // Arrange
        var documentoAlheio = NovoDocumento(Dados.CidadaoB);
        var service = CriarService(FakeUsuarioAtual.Cidadao(Dados.CidadaoA));

        // Act
        var act = () => service.CriarAsync(new CriarEnvioRequest(7, "CIPTEA", [documentoAlheio.Id]));

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(act);
        Assert.Empty(_banco.Envios);
    }

    [Fact]
    public async Task SubmeterAsync_EnvioDoProprioCidadao_DeveFicarEnviado()
    {
        // Arrange
        var envio = NovoEnvio();
        var service = CriarService();

        // Act
        var response = await service.SubmeterAsync(envio.Id);

        // Assert
        Assert.Equal(StatusEnvioEnum.Enviado, response.Status);
    }

    [Fact]
    public async Task SubmeterAsync_EnvioDeOutroCidadao_DeveLancarKeyNotFoundException()
    {
        // Arrange
        var envio = NovoEnvio(Dados.CidadaoB);
        var service = CriarService(FakeUsuarioAtual.Cidadao(Dados.CidadaoA));

        // Act
        var act = () => service.SubmeterAsync(envio.Id);

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(act);
    }

    [Fact]
    public async Task AnalisarAsync_UsuarioNaoGestor_DeveLancarUnauthorizedAccessException()
    {
        // Arrange
        var envio = NovoEnvio();
        envio.Submeter([]);
        var service = CriarService(FakeUsuarioAtual.Cidadao());

        // Act
        var act = () => service.AnalisarAsync(envio.Id, new AnaliseEnvioRequest(DecisaoAnaliseEnum.Aprovar));

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(act);
        Assert.Equal(StatusEnvioEnum.Enviado, envio.Status);
    }

    [Fact]
    public async Task AnalisarAsync_GestorDevolve_DeveRegistrarObservacaoEGestor()
    {
        // Arrange
        var envio = NovoEnvio();
        envio.Submeter([]);
        var service = CriarService(FakeUsuarioAtual.Gestor());

        // Act
        var response = await service.AnalisarAsync(
            envio.Id, new AnaliseEnvioRequest(DecisaoAnaliseEnum.Devolver, "Falta o comprovante de residência."));

        // Assert
        Assert.Equal(StatusEnvioEnum.Devolvido, response.Status);
        Assert.Equal(Dados.Gestor, response.GestorId);
        Assert.Equal("Falta o comprovante de residência.", response.Observacao);
    }

    [Fact]
    public async Task AnalisarAsync_EnvioEmRascunho_DeveLancarRegraDeNegocioException()
    {
        // Arrange
        var envio = NovoEnvio();
        var service = CriarService(FakeUsuarioAtual.Gestor());

        // Act
        var act = () => service.AnalisarAsync(envio.Id, new AnaliseEnvioRequest(DecisaoAnaliseEnum.Aprovar));

        // Assert
        await Assert.ThrowsAsync<RegraDeNegocioException>(act);
    }

    [Fact]
    public async Task ListarAsync_GestorFiltrandoEnviados_DeveRetornarFilaDeAnalise()
    {
        // Arrange
        NovoEnvio(Dados.CidadaoA).Submeter([]);
        NovoEnvio(Dados.CidadaoB).Submeter([]);
        NovoEnvio(Dados.CidadaoB);
        var service = CriarService(FakeUsuarioAtual.Gestor());

        // Act
        var pagina = await service.ListarAsync(new EnvioFiltro(Status: StatusEnvioEnum.Enviado));

        // Assert
        Assert.Equal(2, pagina.TotalItems);
        Assert.All(pagina.Items, e => Assert.Equal(StatusEnvioEnum.Enviado, e.Status));
    }

    [Fact]
    public async Task CancelarAsync_EnvioEmRascunho_DeveCancelar()
    {
        // Arrange
        var envio = NovoEnvio();
        var service = CriarService();

        // Act
        await service.CancelarAsync(envio.Id);

        // Assert
        Assert.Equal(StatusEnvioEnum.Cancelado, envio.Status);
    }

    [Fact]
    public async Task CriarAsync_DireitoForaDoCatalogo_DeveLancarDomainException()
    {
        // Arrange
        var documento = NovoDocumento();
        var service = CriarService();

        // Act
        var act = () => service.CriarAsync(new CriarEnvioRequest(7, "DIREITO_INEXISTENTE", [documento.Id]));

        // Assert
        await Assert.ThrowsAsync<DomainException>(act);
        Assert.Empty(_banco.Envios);
    }

    [Fact]
    public async Task SubmeterAsync_FaltandoDocumentoObrigatorio_DeveLancarRegraDeNegocioException()
    {
        // Arrange: BPC_PCD exige RG e LAUDO, mas o envio só tem RG.
        var envio = new Envio(Dados.CidadaoA, 1, "BPC_PCD", [NovoDocumento()]);
        _banco.Envios.Add(envio);
        var service = CriarService();

        // Act
        var act = () => service.SubmeterAsync(envio.Id);

        // Assert
        var ex = await Assert.ThrowsAsync<RegraDeNegocioException>(act);
        Assert.Contains("Laudo", ex.Message);
        Assert.Equal(StatusEnvioEnum.Rascunho, envio.Status);
    }
}
