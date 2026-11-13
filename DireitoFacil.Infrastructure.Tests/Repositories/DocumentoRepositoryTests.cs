using DireitoFacil.Application.DTOs;
using DireitoFacil.Infrastructure.Repositories;
using DireitoFacil.Infrastructure.Tests.infra;
using DiretoFacil.Domain.Entities;
using DiretoFacil.Domain.Enums;

namespace DireitoFacil.Infrastructure.Tests.Repositories;

public class DocumentoRepositoryTests : IDisposable
{
    private readonly BancoSqlite _banco = new();

    public void Dispose() => _banco.Dispose();

    private async Task GravarAsync(params object[] entidades)
    {
        await using var contexto = _banco.NovoContexto();
        contexto.AddRange(entidades);
        await contexto.SaveChangesAsync();
    }

    [Fact]
    public async Task ExisteHashAsync_DeveConsiderarApenasOProprioCidadao()
    {
        // Arrange
        await GravarAsync(BancoSqlite.Documento(BancoSqlite.CidadaoA, hash: "abc123"));
        await using var contexto = _banco.NovoContexto();
        var repositorio = new DocumentoRepository(contexto);

        // Act + Assert (o hash é comparado em maiúsculas)
        Assert.True(await repositorio.ExisteHashAsync(BancoSqlite.CidadaoA, "ABC123"));
        Assert.False(await repositorio.ExisteHashAsync(BancoSqlite.CidadaoB, "ABC123"));
    }

    [Fact]
    public async Task ObterDoCidadaoAsync_DeveIgnorarDocumentosDeOutroCidadao()
    {
        // Arrange
        var meu = BancoSqlite.Documento(BancoSqlite.CidadaoA);
        var alheio = BancoSqlite.Documento(BancoSqlite.CidadaoB);
        await GravarAsync(meu, alheio);
        await using var contexto = _banco.NovoContexto();
        var repositorio = new DocumentoRepository(contexto);

        // Act
        var resultado = await repositorio.ObterDoCidadaoAsync(BancoSqlite.CidadaoA, [meu.Id, alheio.Id]);

        // Assert
        var documento = Assert.Single(resultado);
        Assert.Equal(meu.Id, documento.Id);
    }

    [Fact]
    public async Task EstaEmEnvioAtivoAsync_EnvioEmRascunho_DeveRetornarTrue()
    {
        // Arrange
        var documento = BancoSqlite.Documento();
        await GravarAsync(documento, new Envio(BancoSqlite.CidadaoA, 1, "CIPTEA", [documento]));
        await using var contexto = _banco.NovoContexto();

        // Act
        var ativo = await new DocumentoRepository(contexto).EstaEmEnvioAtivoAsync(documento.Id);

        // Assert
        Assert.True(ativo);
    }

    [Fact]
    public async Task EstaEmEnvioAtivoAsync_EnvioCancelado_DeveRetornarFalse()
    {
        // Arrange
        var documento = BancoSqlite.Documento();
        var envio = new Envio(BancoSqlite.CidadaoA, 1, "CIPTEA", [documento]);
        envio.Cancelar();
        await GravarAsync(documento, envio);
        await using var contexto = _banco.NovoContexto();

        // Act
        var ativo = await new DocumentoRepository(contexto).EstaEmEnvioAtivoAsync(documento.Id);

        // Assert
        Assert.False(ativo);
    }

    [Fact]
    public async Task GetPagedAsync_DeveFiltrarPorCidadaoETipoEPaginar()
    {
        // Arrange: 3 RG + 1 CPF do cidadão A, 1 RG do cidadão B
        await GravarAsync(
            BancoSqlite.Documento(BancoSqlite.CidadaoA, TipoDocumentoEnum.Rg),
            BancoSqlite.Documento(BancoSqlite.CidadaoA, TipoDocumentoEnum.Rg),
            BancoSqlite.Documento(BancoSqlite.CidadaoA, TipoDocumentoEnum.Rg),
            BancoSqlite.Documento(BancoSqlite.CidadaoA, TipoDocumentoEnum.Cpf),
            BancoSqlite.Documento(BancoSqlite.CidadaoB, TipoDocumentoEnum.Rg));
        await using var contexto = _banco.NovoContexto();
        var repositorio = new DocumentoRepository(contexto);

        // Act
        var (pagina1, total) = await repositorio.GetPagedAsync(
            new DocumentoFiltro(Page: 1, PageSize: 2, Tipo: TipoDocumentoEnum.Rg, CidadaoId: BancoSqlite.CidadaoA));
        var (pagina2, _) = await repositorio.GetPagedAsync(
            new DocumentoFiltro(Page: 2, PageSize: 2, Tipo: TipoDocumentoEnum.Rg, CidadaoId: BancoSqlite.CidadaoA));

        // Assert
        Assert.Equal(3, total);
        Assert.Equal(2, pagina1.Count);
        Assert.Single(pagina2);
        Assert.All(pagina1.Concat(pagina2), d => Assert.Equal(BancoSqlite.CidadaoA, d.CidadaoId));
        Assert.Empty(pagina1.Select(d => d.Id).Intersect(pagina2.Select(d => d.Id)));
    }

    [Fact]
    public async Task AddERemove_SoGravamNoSaveChanges()
    {
        // Arrange
        var documento = BancoSqlite.Documento();
        await using var contexto = _banco.NovoContexto();
        var repositorio = new DocumentoRepository(contexto);

        // Act
        repositorio.Add(documento);
        var antesDeSalvar = await repositorio.ExistsByIdAsync(documento.Id);
        await contexto.SaveChangesAsync();
        var depoisDeSalvar = await repositorio.ExistsByIdAsync(documento.Id);

        // Assert
        Assert.False(antesDeSalvar);
        Assert.True(depoisDeSalvar);
    }
}
