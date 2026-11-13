using DiretoFacil.Domain.Entities;
using DiretoFacil.Domain.Enums;
using DiretoFacil.Domain.Exceptions;

namespace DireitoFacil.Domain.Tests.Domain;

public class DocumentoObrigatorioTests
{
    [Fact]
    public void Construtor_DeveNormalizarCodigoDoDireito()
    {
        // Act
        var obrigatorio = new DocumentoObrigatorio(" ciptea ", TipoDocumentoEnum.Laudo);

        // Assert
        Assert.Equal("CIPTEA", obrigatorio.DireitoCodigo);
        Assert.Equal(TipoDocumentoEnum.Laudo, obrigatorio.TipoDocumento);
    }

    [Fact]
    public void Construtor_SemCodigo_DeveLancarDomainException()
    {
        // Act
        var act = () => new DocumentoObrigatorio("  ", TipoDocumentoEnum.Rg);

        // Assert
        Assert.Throws<DomainException>(act);
    }
}
