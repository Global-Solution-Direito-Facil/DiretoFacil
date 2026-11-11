using DireitoFacil.Application.Tests.Fakes;
using DireitoFacil.Application.Validation;
using DiretoFacil.Domain.Exceptions;

namespace DireitoFacil.Application.Tests.Validation;

public class ArquivoValidatorTests
{
    [Fact]
    public void ValidarAssinatura_PdfVerdadeiro_NaoDeveLancar()
    {
        // Arrange
        using var stream = new MemoryStream(Dados.PdfBytes());

        // Act
        var ex = Record.Exception(() => ArquivoValidator.ValidarAssinatura(stream, "application/pdf"));

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void ValidarAssinatura_PngVerdadeiro_NaoDeveLancar()
    {
        // Arrange
        using var stream = new MemoryStream(Dados.PngBytes());

        // Act
        var ex = Record.Exception(() => ArquivoValidator.ValidarAssinatura(stream, "image/png"));

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void ValidarAssinatura_ExecutavelRenomeadoParaPdf_DeveLancarDomainException()
    {
        // Arrange: "MZ" é o início de um executável do Windows.
        using var stream = new MemoryStream([0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00]);

        // Act
        var act = () => ArquivoValidator.ValidarAssinatura(stream, "application/pdf");

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void ValidarAssinatura_FormatoNaoSuportado_DeveLancarDomainException()
    {
        // Arrange
        using var stream = new MemoryStream(Dados.PdfBytes());

        // Act
        var act = () => ArquivoValidator.ValidarAssinatura(stream, "application/zip");

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void ValidarAssinatura_ArquivoMenorQueAssinatura_DeveLancarDomainException()
    {
        // Arrange
        using var stream = new MemoryStream([0x25, 0x50]);

        // Act
        var act = () => ArquivoValidator.ValidarAssinatura(stream, "application/pdf");

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void ValidarAssinatura_DeveRestaurarPosicaoDoStream()
    {
        // Arrange
        using var stream = new MemoryStream(Dados.PdfBytes());
        stream.Position = 3;

        // Act
        ArquivoValidator.ValidarAssinatura(stream, "application/pdf");

        // Assert
        Assert.Equal(3, stream.Position);
    }
}
