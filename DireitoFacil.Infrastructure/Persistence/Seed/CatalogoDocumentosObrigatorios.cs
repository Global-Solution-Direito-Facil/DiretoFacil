using DiretoFacil.Domain.Enums;

namespace DireitoFacil.Infrastructure.Persistence.Seed;

/// <summary>
/// Documentos exigidos por direito. Os códigos são os mesmos do catálogo no Oracle.
/// PROVISÓRIO: a lista definitiva vem da conferência do catálogo (Losana). Para mudar,
/// edite aqui e gere uma nova migration.
/// </summary>
public static class CatalogoDocumentosObrigatorios
{
    public static readonly IReadOnlyList<(string Direito, TipoDocumentoEnum Tipo)> Itens =
    [
        ("CIPTEA", TipoDocumentoEnum.Laudo),
        ("CIPTEA", TipoDocumentoEnum.Rg),
        ("CIPTEA", TipoDocumentoEnum.Cpf),

        ("ACOMPANHANTE_ESCOLAR_TEA", TipoDocumentoEnum.Laudo),

        ("BPC_PCD", TipoDocumentoEnum.Laudo),
        ("BPC_PCD", TipoDocumentoEnum.Rg),
        ("BPC_PCD", TipoDocumentoEnum.Cpf),
        ("BPC_PCD", TipoDocumentoEnum.ComprovanteRenda),
        ("BPC_PCD", TipoDocumentoEnum.ComprovanteResidencia),

        ("BPC_IDOSO", TipoDocumentoEnum.Rg),
        ("BPC_IDOSO", TipoDocumentoEnum.Cpf),
        ("BPC_IDOSO", TipoDocumentoEnum.ComprovanteRenda),
        ("BPC_IDOSO", TipoDocumentoEnum.ComprovanteResidencia),

        ("CADUNICO", TipoDocumentoEnum.Rg),
        ("CADUNICO", TipoDocumentoEnum.Cpf),
        ("CADUNICO", TipoDocumentoEnum.ComprovanteRenda),
        ("CADUNICO", TipoDocumentoEnum.ComprovanteResidencia),

        ("TARIFA_SOCIAL", TipoDocumentoEnum.Cpf),
        ("TARIFA_SOCIAL", TipoDocumentoEnum.ComprovanteResidencia),

        ("FARMACIA_POPULAR", TipoDocumentoEnum.Receita),
        ("FARMACIA_POPULAR", TipoDocumentoEnum.Rg),
        ("FARMACIA_POPULAR", TipoDocumentoEnum.Cpf),

        ("PASSE_LIVRE_PCD", TipoDocumentoEnum.Laudo),
        ("PASSE_LIVRE_PCD", TipoDocumentoEnum.Rg),
        ("PASSE_LIVRE_PCD", TipoDocumentoEnum.Cpf),
        ("PASSE_LIVRE_PCD", TipoDocumentoEnum.ComprovanteRenda)
    ];
}
