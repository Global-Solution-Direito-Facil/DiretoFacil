using DiretoFacil.Domain.Entites;
using DiretoFacil.Domain.Enums;
using DiretoFacil.Domain.Exceptions;
using Xunit;

namespace DireitoFacil.Domain.Tests;

public class EnvioTests
{
    [Fact]
    public void Construtor_ComDocumentos_DeveCriarEmRascunho()
    {
        // Act
        var envio = Fakes.Envio(Fakes.Documento(), Fakes.Documento());

        // Assert
        Assert.Equal(StatusEnvioEnum.Rascunho, envio.Status);
        Assert.Equal("CIPTEA", envio.DireitoCodigo);
        Assert.Equal(2, envio.Documentos.Count);
        Assert.All(envio.Documentos, ed => Assert.Equal(envio.Id, ed.EnvioId));
    }

    [Fact]
    public void Construtor_SemDocumentos_DeveLancarDomainException()
    {
        // Act
        var act = () => new Envio(Fakes.CidadaoA, 10, "CIPTEA", []);

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Construtor_ComCasoInvalido_DeveLancarDomainException(long casoId)
    {
        // Act
        var act = () => new Envio(Fakes.CidadaoA, casoId, "CIPTEA", [Fakes.Documento()]);

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Construtor_ComDocumentoDeOutroCidadao_DeveLancarDomainException()
    {
        // Act
        var act = () => Fakes.Envio(Fakes.Documento(Fakes.CidadaoB));

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Construtor_ComDocumentoRepetido_DeveManterApenasUm()
    {
        // Arrange
        var documento = Fakes.Documento();

        // Act
        var envio = Fakes.Envio(documento, documento);

        // Assert
        Assert.Single(envio.Documentos);
    }

    [Fact]
    public void Construtor_ComDocumentoQueFalhou_DeveLancarRegraDeNegocioException()
    {
        // Arrange
        var documento = Fakes.Documento();
        documento.MarcarFalha();

        // Act
        var act = () => Fakes.Envio(documento);

        // Assert
        Assert.Throws<RegraDeNegocioException>(act);
    }

    [Fact]
    public void Submeter_EmRascunho_DeveMudarParaEnviado()
    {
        // Arrange
        var envio = Fakes.Envio();

        // Act
        envio.Submeter(Fakes.SemObrigatorios);

        // Assert
        Assert.Equal(StatusEnvioEnum.Enviado, envio.Status);
        Assert.NotNull(envio.EnviadoEm);
    }

    [Fact]
    public void Submeter_EnvioJaEnviado_DeveLancarRegraDeNegocioException()
    {
        // Arrange
        var envio = Fakes.EnvioEnviado();

        // Act
        var act = () => envio.Submeter(Fakes.SemObrigatorios);

        // Assert
        Assert.Throws<RegraDeNegocioException>(act);
    }

    [Fact]
    public void Submeter_ComDocumentoQueFalhouDepois_DeveLancarRegraDeNegocioException()
    {
        // Arrange
        var documento = Fakes.Documento();
        var envio = Fakes.Envio(documento);
        documento.MarcarFalha();

        // Act
        var act = () => envio.Submeter(Fakes.SemObrigatorios);

        // Assert
        Assert.Throws<RegraDeNegocioException>(act);
    }

    [Theory]
    [InlineData((string?)null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Devolver_SemObservacao_DeveLancarDomainException(string? observacao)
    {
        // Arrange
        var envio = Fakes.EnvioEnviado();

        // Act
        var act = () => envio.Analisar(DecisaoAnaliseEnum.Devolver, Fakes.Gestor, observacao);

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Aprovar_SemGestor_DeveLancarDomainException()
    {
        // Arrange
        var envio = Fakes.EnvioEnviado();

        // Act
        var act = () => envio.Aprovar(" ");

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Aprovar_EnvioEmRascunho_DeveLancarRegraDeNegocioException()
    {
        // Arrange
        var envio = Fakes.Envio();

        // Act
        var act = () => envio.Aprovar(Fakes.Gestor);

        // Assert
        Assert.Throws<RegraDeNegocioException>(act);
    }

    [Fact]
    public void FluxoCompleto_DevolverCorrigirSubmeterAprovar_DeveTerminarAprovado()
    {
        // Arrange
        var envio = Fakes.EnvioEnviado();

        // Act
        envio.Devolver(Fakes.Gestor, "Falta o comprovante de residência.");
        envio.DefinirDocumentos([Fakes.Documento(), Fakes.Documento(tipo: TipoDocumentoEnum.ComprovanteResidencia)]);
        envio.Submeter(Fakes.SemObrigatorios);
        envio.Analisar(DecisaoAnaliseEnum.Aprovar, Fakes.Gestor, null);

        // Assert
        Assert.Equal(StatusEnvioEnum.Aprovado, envio.Status);
        Assert.Equal(Fakes.Gestor, envio.GestorId);
        Assert.NotNull(envio.AnalisadoEm);
    }

    [Fact]
    public void DefinirDocumentos_EnvioEnviado_DeveLancarRegraDeNegocioException()
    {
        // Arrange
        var envio = Fakes.EnvioEnviado();

        // Act
        var act = () => envio.DefinirDocumentos([Fakes.Documento()]);

        // Assert
        Assert.Throws<RegraDeNegocioException>(act);
    }

    [Fact]
    public void DefinirDocumentos_ComMesmoDocumento_NaoDeveDuplicarAssociacao()
    {
        // Arrange
        var documento = Fakes.Documento();
        var envio = Fakes.Envio(documento);
        var associacaoOriginal = envio.Documentos.Single();

        // Act
        envio.DefinirDocumentos([documento, Fakes.Documento()]);

        // Assert
        Assert.Equal(2, envio.Documentos.Count);
        Assert.Contains(associacaoOriginal, envio.Documentos);
    }

    [Fact]
    public void DefinirDocumentos_SemODocumentoAntigo_DeveRemoverAssociacao()
    {
        // Arrange
        var antigo = Fakes.Documento();
        var novo = Fakes.Documento();
        var envio = Fakes.Envio(antigo);

        // Act
        envio.DefinirDocumentos([novo]);

        // Assert
        var associacao = Assert.Single(envio.Documentos);
        Assert.Equal(novo.Id, associacao.DocumentoId);
    }

    [Fact]
    public void Cancelar_EnvioEnviado_DeveLancarRegraDeNegocioException()
    {
        // Arrange
        var envio = Fakes.EnvioEnviado();

        // Act
        var act = envio.Cancelar;

        // Assert
        Assert.Throws<RegraDeNegocioException>(act);
    }

    [Fact]
    public void Cancelar_EmRascunho_DeveFicarInativo()
    {
        // Arrange
        var envio = Fakes.Envio();

        // Act
        envio.Cancelar();

        // Assert
        Assert.Equal(StatusEnvioEnum.Cancelado, envio.Status);
        Assert.False(envio.EstaAtivo());
    }

    [Fact]
    public void Submeter_FaltandoDocumentoObrigatorio_DeveLancarRegraDeNegocioException()
    {
        // Arrange: o envio só tem RG, mas o direito exige RG e LAUDO.
        var envio = Fakes.Envio(Fakes.Documento(tipo: TipoDocumentoEnum.Rg));

        // Act
        var act = () => envio.Submeter([TipoDocumentoEnum.Rg, TipoDocumentoEnum.Laudo]);

        // Assert
        var ex = Assert.Throws<RegraDeNegocioException>(act);
        Assert.Contains("Laudo", ex.Message);
        Assert.Equal(StatusEnvioEnum.Rascunho, envio.Status);
    }

    [Fact]
    public void Submeter_ComTodosOsObrigatorios_DeveMudarParaEnviado()
    {
        // Arrange
        var envio = Fakes.Envio(
            Fakes.Documento(tipo: TipoDocumentoEnum.Rg),
            Fakes.Documento(tipo: TipoDocumentoEnum.Laudo, consentimento: true));

        // Act
        envio.Submeter([TipoDocumentoEnum.Rg, TipoDocumentoEnum.Laudo]);

        // Assert
        Assert.Equal(StatusEnvioEnum.Enviado, envio.Status);
    }

    [Fact]
    public void Submeter_AposDevolucaoCorrigindoDocumentoFaltante_DeveMudarParaEnviado()
    {
        // Arrange: devolvido porque faltava o comprovante de residência.
        var rg = Fakes.Documento(tipo: TipoDocumentoEnum.Rg);
        var envio = Fakes.Envio(rg);
        envio.Submeter(Fakes.SemObrigatorios);
        envio.Devolver(Fakes.Gestor, "Falta o comprovante de residência.");
        TipoDocumentoEnum[] obrigatorios = [TipoDocumentoEnum.Rg, TipoDocumentoEnum.ComprovanteResidencia];

        // Act
        envio.DefinirDocumentos([rg, Fakes.Documento(tipo: TipoDocumentoEnum.ComprovanteResidencia)]);
        envio.Submeter(obrigatorios);

        // Assert
        Assert.Equal(StatusEnvioEnum.Enviado, envio.Status);
    }

    [Fact]
    public void Construtor_DeveNormalizarCodigoDoDireito()
    {
        // Act
        var envio = new Envio(Fakes.CidadaoA, 10, "  bpc_pcd ", [Fakes.Documento()]);

        // Assert
        Assert.Equal("BPC_PCD", envio.DireitoCodigo);
    }
}
