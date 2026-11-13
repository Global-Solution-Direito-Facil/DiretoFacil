using DireitoFacil.Domain.Enums;
using DireitoFacil.Domain.Exceptions;

namespace DireitoFacil.Domain.Tests.Domain;

public class DocumentoTests
{
    [Fact]
    public void Construtor_ComDadosValidos_DeveCriarComStatusRecebido()
    {
        // Act
        var documento = Fakes.Documento();

        // Assert
        Assert.NotEqual(Guid.Empty, documento.Id);
        Assert.Equal(StatusDocumentoEnum.Recebido, documento.Status);
        Assert.Equal($"{Fakes.CidadaoA}/{documento.Id:N}", documento.BlobPath);
    }

    [Fact]
    public void Construtor_DoisDocumentos_DevemTerIdsDiferentes()
    {
        // Act
        var primeiro = Fakes.Documento();
        var segundo = Fakes.Documento();

        // Assert
        Assert.NotEqual(primeiro.Id, segundo.Id);
    }

    [Fact]
    public void Construtor_LaudoSemConsentimento_DeveLancarDomainException()
    {
        // Act
        var act = () => Fakes.Documento(tipo: TipoDocumentoEnum.Laudo, consentimento: false);

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Construtor_LaudoComConsentimento_DeveCriar()
    {
        // Act
        var documento = Fakes.Documento(tipo: TipoDocumentoEnum.Laudo, consentimento: true);

        // Assert
        Assert.True(documento.ConsentimentoSaude);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10 * 1024 * 1024 + 1)]
    public void Construtor_ComTamanhoInvalido_DeveLancarDomainException(long tamanho)
    {
        // Act
        var act = () => Fakes.Documento(tamanho: tamanho);

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Construtor_ComFormatoNaoPermitido_DeveLancarDomainException()
    {
        // Act
        var act = () => Fakes.Documento(contentType: "application/zip");

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Reclassificar_DuranteProcessamento_DeveLancarRegraDeNegocioException()
    {
        // Arrange
        var documento = Fakes.Documento();
        documento.IniciarProcessamento();

        // Act
        var act = () => documento.Reclassificar(TipoDocumentoEnum.Cpf, null, false);

        // Assert
        Assert.Throws<RegraDeNegocioException>(act);
    }

    [Fact]
    public void Reclassificar_ParaLaudoSemConsentimento_DeveLancarDomainException()
    {
        // Arrange
        var documento = Fakes.Documento();

        // Act
        var act = () => documento.Reclassificar(TipoDocumentoEnum.Laudo, null, false);

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Reclassificar_ComDadosValidos_DeveAtualizarTipoEData()
    {
        // Arrange
        var documento = Fakes.Documento();

        // Act
        documento.Reclassificar(TipoDocumentoEnum.Cpf, "  CPF do responsável  ", false);

        // Assert
        Assert.Equal(TipoDocumentoEnum.Cpf, documento.Tipo);
        Assert.Equal("CPF do responsável", documento.Descricao);
        Assert.NotNull(documento.AtualizadoEm);
    }

    [Fact]
    public void MarcarExtraido_ChamadoDuasVezes_DeveSerIdempotente()
    {
        // Arrange
        var documento = Fakes.Documento();
        documento.IniciarProcessamento();
        documento.MarcarExtraido();

        // Act
        documento.MarcarExtraido();

        // Assert
        Assert.Equal(StatusDocumentoEnum.Extraido, documento.Status);
    }

    [Fact]
    public void IniciarProcessamento_DocumentoJaExtraido_DeveLancarRegraDeNegocioException()
    {
        // Arrange
        var documento = Fakes.Documento();
        documento.MarcarExtraido();

        // Act
        var act = documento.IniciarProcessamento;

        // Assert
        Assert.Throws<RegraDeNegocioException>(act);
    }

    [Fact]
    public void AtualizarStatus_ParaRecebido_DeveLancarDomainException()
    {
        // Arrange
        var documento = Fakes.Documento();

        // Act
        var act = () => documento.AtualizarStatus(StatusDocumentoEnum.Recebido);

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void PertenceA_OutroCidadao_DeveRetornarFalse()
    {
        // Arrange
        var documento = Fakes.Documento(Fakes.CidadaoA);

        // Act + Assert
        Assert.False(documento.PertenceA(Fakes.CidadaoB));
    }

    [Fact]
    public void Construtor_ComConsentimento_DeveRegistrarDataDoConsentimento()
    {
        // Act
        var documento = Fakes.Documento(tipo: TipoDocumentoEnum.Laudo, consentimento: true);

        // Assert
        Assert.NotNull(documento.ConsentimentoSaudeEm);
    }

    [Fact]
    public void Construtor_SemConsentimento_NaoDeveTerDataDeConsentimento()
    {
        // Act
        var documento = Fakes.Documento(consentimento: false);

        // Assert
        Assert.Null(documento.ConsentimentoSaudeEm);
    }

    [Fact]
    public void Reclassificar_ParaLaudoComConsentimento_DeveRegistrarDataDoConsentimento()
    {
        // Arrange
        var documento = Fakes.Documento(tipo: TipoDocumentoEnum.Rg, consentimento: false);

        // Act
        documento.Reclassificar(TipoDocumentoEnum.Laudo, null, consentimentoSaude: true);

        // Assert
        Assert.True(documento.ConsentimentoSaude);
        Assert.NotNull(documento.ConsentimentoSaudeEm);
    }

    [Fact]
    public void Reclassificar_ConsentimentoJaDado_DeveManterDataOriginal()
    {
        // Arrange
        var documento = Fakes.Documento(tipo: TipoDocumentoEnum.Laudo, consentimento: true);
        var dataOriginal = documento.ConsentimentoSaudeEm;

        // Act
        documento.Reclassificar(TipoDocumentoEnum.Laudo, "Laudo atualizado", consentimentoSaude: true);

        // Assert
        Assert.Equal(dataOriginal, documento.ConsentimentoSaudeEm);
    }
}
