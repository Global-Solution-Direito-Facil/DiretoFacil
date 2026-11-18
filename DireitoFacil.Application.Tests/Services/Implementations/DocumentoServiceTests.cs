using DireitoFacil.Application.DTOs;
using DireitoFacil.Application.Events;
using DireitoFacil.Application.Services.Implementations;
using DireitoFacil.Application.Tests.Fakes;
using DireitoFacil.Domain.Entities;
using DireitoFacil.Domain.Enums;
using DireitoFacil.Domain.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;

namespace DireitoFacil.Application.Tests.Services.Implementations;

public class DocumentoServiceTests
{
    private readonly BancoEmMemoria _banco = new();
    private readonly FakeArquivoStorage _storage = new();
    private readonly FakeEventPublisher _eventos = new();

    private DocumentoService CriarService(FakeUsuarioAtual? usuario = null) => new(
        new FakeDocumentoRepository(_banco),
        _storage,
        _eventos,
        _banco,
        usuario ?? FakeUsuarioAtual.Cidadao(),
        NullLogger<DocumentoService>.Instance);

    private static CriarDocumentoRequest RequestLaudo(bool consentimento = true)
        => new(TipoDocumentoEnum.Laudo, consentimento, "Laudo de TEA");

    [Fact]
    public async Task CriarAsync_ComPdfValido_DeveSalvarArquivoRegistrarEPublicarEvento()
    {
        // Arrange
        var service = CriarService();

        // Act
        var response = await service.CriarAsync(RequestLaudo(), Dados.Upload(Dados.PdfBytes()));

        // Assert
        var documento = Assert.Single(_banco.Documentos);
        Assert.Equal(response.Id, documento.Id);
        Assert.Equal(Dados.CidadaoA, documento.CidadaoId);
        Assert.Equal(StatusDocumentoEnum.Recebido, response.Status);
        Assert.True(_storage.Arquivos.ContainsKey(documento.BlobPath));
        Assert.Equal(1, _banco.Salvamentos);

        var evento = Assert.IsType<DocumentoRecebidoEvent>(Assert.Single(_eventos.Eventos));
        Assert.Equal(documento.Id, evento.DocumentoId);
    }

    [Fact]
    public async Task CriarAsync_ComPessoaId_DeveGuardarNoDocumentoEColocarNoEvento()
    {
        // Arrange
        var service = CriarService();
        var request = new CriarDocumentoRequest(TipoDocumentoEnum.Laudo, true, "Laudo do Lucas", PessoaId: 7);

        // Act
        var response = await service.CriarAsync(request, Dados.Upload(Dados.PdfBytes()));

        // Assert
        Assert.Equal<long?>(7L, response.PessoaId);
        Assert.Equal<long?>(7L, Assert.Single(_banco.Documentos).PessoaId);
        var evento = Assert.IsType<DocumentoRecebidoEvent>(Assert.Single(_eventos.Eventos));
        Assert.Equal<long?>(7L, evento.PessoaId);
    }

    [Fact]
    public async Task CriarAsync_MesmoArquivoDuasVezes_DeveLancarRegraDeNegocioException()
    {
        // Arrange
        var service = CriarService();
        await service.CriarAsync(RequestLaudo(), Dados.Upload(Dados.PdfBytes()));

        // Act
        var act = () => service.CriarAsync(RequestLaudo(), Dados.Upload(Dados.PdfBytes()));

        // Assert
        await Assert.ThrowsAsync<RegraDeNegocioException>(act);
        Assert.Single(_banco.Documentos);
    }

    [Fact]
    public async Task CriarAsync_PngDeclaradoComoPdf_DeveLancarDomainException()
    {
        // Arrange
        var service = CriarService();

        // Act
        var act = () => service.CriarAsync(RequestLaudo(), Dados.Upload(Dados.PngBytes(), "application/pdf"));

        // Assert
        await Assert.ThrowsAsync<DomainException>(act);
        Assert.Empty(_storage.Arquivos);
    }

    [Fact]
    public async Task CriarAsync_LaudoSemConsentimento_DeveLancarDomainExceptionSemGravarArquivo()
    {
        // Arrange
        var service = CriarService();

        // Act
        var act = () => service.CriarAsync(RequestLaudo(consentimento: false), Dados.Upload(Dados.PdfBytes()));

        // Assert
        await Assert.ThrowsAsync<DomainException>(act);
        Assert.Empty(_storage.Arquivos);
        Assert.Empty(_eventos.Eventos);
    }

    [Fact]
    public async Task CriarAsync_FalhaAoSalvarNoBanco_DeveRemoverArquivoDoStorage()
    {
        // Arrange
        _banco.FalharAoSalvar = true;
        var service = CriarService();

        // Act
        var act = () => service.CriarAsync(RequestLaudo(), Dados.Upload(Dados.PdfBytes()));

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Empty(_storage.Arquivos);
    }

    [Fact]
    public async Task ObterAsync_DocumentoDeOutroCidadao_DeveLancarKeyNotFoundException()
    {
        // Arrange
        var documento = Dados.Documento(Dados.CidadaoB);
        _banco.Documentos.Add(documento);
        var service = CriarService(FakeUsuarioAtual.Cidadao(Dados.CidadaoA));

        // Act
        var act = () => service.ObterAsync(documento.Id);

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(act);
    }

    [Fact]
    public async Task ObterAsync_Gestor_DeveVerDocumentoDeQualquerCidadao()
    {
        // Arrange
        var documento = Dados.Documento(Dados.CidadaoB);
        _banco.Documentos.Add(documento);
        var service = CriarService(FakeUsuarioAtual.Gestor());

        // Act
        var response = await service.ObterAsync(documento.Id);

        // Assert
        Assert.Equal(documento.Id, response.Id);
    }

    [Fact]
    public async Task ListarAsync_CidadaoFiltrandoOutroCidadao_DeveRetornarSoOsProprios()
    {
        // Arrange
        _banco.Documentos.AddRange([Dados.Documento(Dados.CidadaoA), Dados.Documento(Dados.CidadaoB), Dados.Documento(Dados.CidadaoB)]);
        var service = CriarService(FakeUsuarioAtual.Cidadao(Dados.CidadaoA));

        // Act
        var pagina = await service.ListarAsync(new DocumentoFiltro(CidadaoId: Dados.CidadaoB));

        // Assert
        Assert.Equal(1, pagina.TotalItems);
        Assert.All(pagina.Items, d => Assert.Equal(Dados.CidadaoA, d.CidadaoId));
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task ListarAsync_PaginacaoInvalida_DeveLancarArgumentException(int page, int pageSize)
    {
        // Arrange
        var service = CriarService();

        // Act
        var act = () => service.ListarAsync(new DocumentoFiltro(page, pageSize));

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task ExcluirAsync_DocumentoEmEnvioAtivo_DeveLancarRegraDeNegocioException()
    {
        // Arrange
        var documento = Dados.Documento();
        _banco.Documentos.Add(documento);
        _banco.Envios.Add(new Envio(Dados.CidadaoA, 1, "CIPTEA", [documento]));
        var service = CriarService();

        // Act
        var act = () => service.ExcluirAsync(documento.Id);

        // Assert
        await Assert.ThrowsAsync<RegraDeNegocioException>(act);
        Assert.Single(_banco.Documentos);
    }

    [Fact]
    public async Task ExcluirAsync_DocumentoLivre_DeveRemoverRegistroEArquivo()
    {
        // Arrange
        var service = CriarService();
        var criado = await service.CriarAsync(RequestLaudo(), Dados.Upload(Dados.PdfBytes()));

        // Act
        await service.ExcluirAsync(criado.Id);

        // Assert
        Assert.Empty(_banco.Documentos);
        Assert.Empty(_storage.Arquivos);
    }

    [Fact]
    public async Task ExcluirAsync_DocumentoDeOutroCidadao_DeveLancarKeyNotFoundException()
    {
        // Arrange
        var documento = Dados.Documento(Dados.CidadaoB);
        _banco.Documentos.Add(documento);
        var service = CriarService(FakeUsuarioAtual.Cidadao(Dados.CidadaoA));

        // Act
        var act = () => service.ExcluirAsync(documento.Id);

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(act);
        Assert.Single(_banco.Documentos);
    }

    [Fact]
    public async Task AtualizarStatusAsync_Extraido_DeveAtualizarDocumento()
    {
        // Arrange
        var documento = Dados.Documento();
        _banco.Documentos.Add(documento);
        var service = CriarService(new FakeUsuarioAtual { Id = "api-java", EhServico = true });

        // Act
        await service.AtualizarStatusAsync(documento.Id, new AtualizarStatusDocumentoRequest(StatusDocumentoEnum.Extraido));

        // Assert
        Assert.Equal(StatusDocumentoEnum.Extraido, documento.Status);
        Assert.Equal(1, _banco.Salvamentos);
    }
}
