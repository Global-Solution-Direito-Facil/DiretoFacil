using DireitoFacil.Domain.Enums;
using DireitoFacil.Domain.Exceptions;

namespace DireitoFacil.Domain.Entities;

/// <summary>
/// Documento exigido por um direito (ex.: CIPTEA exige LAUDO, RG e foto).
/// Os códigos são os mesmos do catálogo de direitos no Oracle (CIPTEA, BPC_PCD, CADUNICO...).
/// Não herda de BaseEntity: a chave é composta (DireitoCodigo, TipoDocumento).
/// </summary>
public class DocumentoObrigatorio
{
    public string DireitoCodigo { get; private set; } = null!;
    public TipoDocumentoEnum TipoDocumento { get; private set; }

    protected DocumentoObrigatorio()
    {
    }

    public DocumentoObrigatorio(string direitoCodigo, TipoDocumentoEnum tipoDocumento)
    {
        if (!Enum.IsDefined(tipoDocumento))
            throw new DomainException("Tipo de documento inválido.");

        DireitoCodigo = NormalizarCodigo(direitoCodigo);
        TipoDocumento = tipoDocumento;
    }

    /// <summary>Códigos sempre em maiúsculas e sem espaços nas pontas ("ciptea " → "CIPTEA").</summary>
    public static string NormalizarCodigo(string direitoCodigo)
    {
        if (string.IsNullOrWhiteSpace(direitoCodigo))
            throw new DomainException("O código do direito é obrigatório.");

        return direitoCodigo.Trim().ToUpperInvariant();
    }
}
