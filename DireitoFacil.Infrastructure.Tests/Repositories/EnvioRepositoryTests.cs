using DireitoFacil.Application.DTOs;
using DireitoFacil.Infrastructure.Repositories;
using DireitoFacil.Infrastructure.Tests.Infra;
using DiretoFacil.Domain.Entites;
using DiretoFacil.Domain.Enums;

namespace DireitoFacil.Infrastructure.Tests.Repositories;

public class EnvioRepositoryTests : IDisposable
{
    private readonly BancoSqlite _banco = new();

    public void Dispose() => _banco.Dispose();

    private async Task<Envio> GravarEnvioAsync(string cidadaoId = BancoSqlite.CidadaoA, bool submeter = false)
    {
        var documento = BancoSqlite.Documento(cidadaoId);
        var envio = new Envio(cidadaoId, 1, "CIPTEA", [documento]);
        if (submeter)
            envio.Submeter([]);

        await using var contexto = _banco.NovoContexto();
        contexto.AddRange(documento, envio);
        await contexto.SaveChangesAsync();
        return envio;
    }

    [Fact]
    public async Task ObterComDocumentosAsync_DeveCarregarOsDocumentos()
    {
        // Arrange
        var envio = await GravarEnvioAsync();
        await using var contexto = _banco.NovoContexto();

        // Act
        var carregado = await new EnvioRepository(contexto).ObterComDocumentosAsync(envio.Id);

        // Assert
        Assert.NotNull(carregado);
        var vinculo = Assert.Single(carregado.Documentos);
        Assert.NotNull(vinculo.Documento);
        Assert.Equal(TipoDocumentoEnum.Rg, vinculo.Documento.Tipo);
    }

    [Fact]
    public async Task ObterComDocumentosAsync_DevePermitirSubmeterEGravarNovoStatus()
    {
        // Arrange
        var envio = await GravarEnvioAsync();

        // Act
        await using (var contexto = _banco.NovoContexto())
        {
            var carregado = await new EnvioRepository(contexto).ObterComDocumentosAsync(envio.Id);
            carregado!.Submeter([TipoDocumentoEnum.Rg]);
            await contexto.SaveChangesAsync();
        }

        // Assert
        await using var verificacao = _banco.NovoContexto();
        var gravado = await new EnvioRepository(verificacao).GetByIdAsync(envio.Id);
        Assert.Equal(StatusEnvioEnum.Enviado, gravado!.Status);
        Assert.NotNull(gravado.EnviadoEm);
    }

    [Fact]
    public async Task GetPagedAsync_FiltrandoEnviados_DeveRetornarFilaComIdsDosDocumentos()
    {
        // Arrange
        await GravarEnvioAsync(BancoSqlite.CidadaoA, submeter: true);
        await GravarEnvioAsync(BancoSqlite.CidadaoB, submeter: true);
        await GravarEnvioAsync(BancoSqlite.CidadaoB, submeter: false);
        await using var contexto = _banco.NovoContexto();

        // Act
        var (itens, total) = await new EnvioRepository(contexto)
            .GetPagedAsync(new EnvioFiltro(Status: StatusEnvioEnum.Enviado));

        // Assert
        Assert.Equal(2, total);
        Assert.All(itens, e => Assert.Single(e.Documentos));
    }

    [Fact]
    public async Task DefinirDocumentos_TrocandoDocumento_DeveRemoverVinculoAntigoNoBanco()
    {
        // Arrange
        var envio = await GravarEnvioAsync();
        var novo = BancoSqlite.Documento(tipo: TipoDocumentoEnum.Cpf);

        // Act
        await using (var contexto = _banco.NovoContexto())
        {
            contexto.Documentos.Add(novo);
            var carregado = await new EnvioRepository(contexto).ObterComDocumentosAsync(envio.Id);
            carregado!.DefinirDocumentos([novo]);
            await contexto.SaveChangesAsync();
        }

        // Assert
        await using var verificacao = _banco.NovoContexto();
        var gravado = await new EnvioRepository(verificacao).ObterComDocumentosAsync(envio.Id);
        var vinculo = Assert.Single(gravado!.Documentos);
        Assert.Equal(novo.Id, vinculo.DocumentoId);
    }
}
