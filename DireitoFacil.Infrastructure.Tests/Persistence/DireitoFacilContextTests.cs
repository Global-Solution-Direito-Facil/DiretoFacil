using DireitoFacil.Application.Repositories;
using DireitoFacil.Infrastructure.Tests.Infra;
using DiretoFacil.Domain.Entites;
using DiretoFacil.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DireitoFacil.Infrastructure.Tests.Persistence;

public class DireitoFacilContextTests : IDisposable
{
    private readonly BancoSqlite _banco = new();

    public void Dispose() => _banco.Dispose();

    [Fact]
    public async Task UnitOfWork_SaveChanges_DeveGravarEntidade()
    {
        // Arrange
        var documento = BancoSqlite.Documento();
        await using (var contexto = _banco.NovoContexto())
        {
            contexto.Documentos.Add(documento);

            // Act: grava pela interface usada pela Application
            await ((IUnitOfWork)contexto).SaveChangesAsync();
        }

        // Assert
        await using var verificacao = _banco.NovoContexto();
        Assert.True(await verificacao.Documentos.AnyAsync(d => d.Id == documento.Id));
    }

    [Fact]
    public async Task Envio_DeveGravarERecarregarAssociacoesPeloCampoPrivado()
    {
        // Arrange
        var rg = BancoSqlite.Documento(tipo: TipoDocumentoEnum.Rg);
        var cpf = BancoSqlite.Documento(tipo: TipoDocumentoEnum.Cpf);
        var envio = new Envio(BancoSqlite.CidadaoA, 1, "CIPTEA", [rg, cpf]);

        await using (var contexto = _banco.NovoContexto())
        {
            contexto.Documentos.AddRange(rg, cpf);
            contexto.Envios.Add(envio);
            await contexto.SaveChangesAsync();
        }

        // Act
        await using var verificacao = _banco.NovoContexto();
        var carregado = await verificacao.Envios.Include(e => e.Documentos).SingleAsync(e => e.Id == envio.Id);

        // Assert
        Assert.Equal(2, carregado.Documentos.Count);
        Assert.Equal(StatusEnvioEnum.Rascunho, carregado.Status);
        Assert.Equal("CIPTEA", carregado.DireitoCodigo);
    }

    [Fact]
    public async Task Documento_MesmoHashParaMesmoCidadao_DeveSerBarradoPeloIndiceUnico()
    {
        // Arrange
        await using var contexto = _banco.NovoContexto();
        contexto.Documentos.Add(BancoSqlite.Documento(hash: "ABC"));
        await contexto.SaveChangesAsync();

        // Act
        contexto.Documentos.Add(BancoSqlite.Documento(hash: "ABC"));
        var act = () => contexto.SaveChangesAsync();

        // Assert
        await Assert.ThrowsAsync<DbUpdateException>(act);
    }

    [Fact]
    public async Task Documento_MesmoHashParaCidadaosDiferentes_DevePermitir()
    {
        // Arrange
        await using var contexto = _banco.NovoContexto();
        contexto.Documentos.Add(BancoSqlite.Documento(BancoSqlite.CidadaoA, hash: "ABC"));
        contexto.Documentos.Add(BancoSqlite.Documento(BancoSqlite.CidadaoB, hash: "ABC"));

        // Act
        await contexto.SaveChangesAsync();

        // Assert
        Assert.Equal(2, await contexto.Documentos.CountAsync());
    }

    [Fact]
    public async Task Usuario_EmailDuplicado_DeveSerBarradoPeloIndiceUnico()
    {
        // Arrange
        await using var contexto = _banco.NovoContexto();
        contexto.Usuarios.Add(new Usuario("Maria", "maria@teste.com", "hash"));
        await contexto.SaveChangesAsync();

        // Act
        contexto.Usuarios.Add(new Usuario("Outra Maria", "MARIA@teste.com", "hash"));
        var act = () => contexto.SaveChangesAsync();

        // Assert
        await Assert.ThrowsAsync<DbUpdateException>(act);
    }

    [Fact]
    public async Task Seed_DeveCarregarCatalogoDeDocumentosObrigatorios()
    {
        // Act
        await using var contexto = _banco.NovoContexto();
        var tiposCiptea = await contexto.DocumentosObrigatorios
            .Where(o => o.DireitoCodigo == "CIPTEA")
            .Select(o => o.TipoDocumento)
            .ToListAsync();

        // Assert
        Assert.Contains(TipoDocumentoEnum.Laudo, tiposCiptea);
        Assert.Contains(TipoDocumentoEnum.Rg, tiposCiptea);
    }
}
