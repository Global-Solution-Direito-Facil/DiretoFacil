using DireitoFacil.Infrastructure.Repositories;
using DireitoFacil.Infrastructure.Tests.infra;
using DireitoFacil.Domain.Entities;
using DireitoFacil.Domain.Enums;

namespace DireitoFacil.Infrastructure.Tests.Repositories;

public class UsuarioEObrigatorioRepositoryTests : IDisposable
{
    private readonly BancoSqlite _banco = new();

    public void Dispose() => _banco.Dispose();

    [Fact]
    public async Task UsuarioRepository_GetByEmailAsync_DeveEncontrarPeloEmailNormalizado()
    {
        // Arrange
        await using (var contexto = _banco.NovoContexto())
        {
            contexto.Usuarios.Add(new Usuario("Maria", "Maria@Teste.com", "hash", PapelUsuarioEnum.Gestor));
            await contexto.SaveChangesAsync();
        }

        await using var leitura = _banco.NovoContexto();
        var repositorio = new UsuarioRepository(leitura);

        // Act
        var usuario = await repositorio.GetByEmailAsync("maria@teste.com");

        // Assert
        Assert.NotNull(usuario);
        Assert.Equal(PapelUsuarioEnum.Gestor, usuario.Papel);
        Assert.True(await repositorio.ExisteEmailAsync("maria@teste.com"));
        Assert.False(await repositorio.ExisteEmailAsync("outra@teste.com"));
    }

    [Theory]
    [InlineData("CIPTEA")]
    [InlineData(" ciptea ")]
    public async Task DocumentoObrigatorioRepository_DireitoExisteAsync_DeveNormalizarCodigo(string codigo)
    {
        // Arrange
        await using var contexto = _banco.NovoContexto();

        // Act
        var existe = await new DocumentoObrigatorioRepository(contexto).DireitoExisteAsync(codigo);

        // Assert
        Assert.True(existe);
    }

    [Fact]
    public async Task DocumentoObrigatorioRepository_DireitoInexistente_DeveRetornarFalseELista()
    {
        // Arrange
        await using var contexto = _banco.NovoContexto();
        var repositorio = new DocumentoObrigatorioRepository(contexto);

        // Act + Assert
        Assert.False(await repositorio.DireitoExisteAsync("NAO_EXISTE"));
        Assert.Empty(await repositorio.ObterTiposAsync("NAO_EXISTE"));
    }

    [Fact]
    public async Task DocumentoObrigatorioRepository_ObterTiposAsync_DeveRetornarTiposDoCatalogo()
    {
        // Arrange
        await using var contexto = _banco.NovoContexto();

        // Act
        var tipos = await new DocumentoObrigatorioRepository(contexto).ObterTiposAsync("BPC_PCD");

        // Assert
        Assert.Contains(TipoDocumentoEnum.Laudo, tipos);
        Assert.Contains(TipoDocumentoEnum.ComprovanteRenda, tipos);
    }
}
