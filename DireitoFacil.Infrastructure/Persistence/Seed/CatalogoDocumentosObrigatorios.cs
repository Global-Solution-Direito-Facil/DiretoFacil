using DireitoFacil.Domain.Enums;

namespace DireitoFacil.Infrastructure.Persistence.Seed;

/// <summary>
/// Documentos exigidos por direito, para os 20 direitos do catálogo do Oracle (T_DF_DIREITO).
/// Os códigos são os mesmos do COD_DIREITO e a lista de documentos segue a base de conhecimento
/// da API Java (conhecimento/direitos.md, seção "Como pedir" de cada direito).
/// Farmácia Popular não entra: é uma consulta de medicamentos e não gera caso nem envio.
/// Para mudar, edite aqui e gere uma nova migration.
/// </summary>
public static class CatalogoDocumentosObrigatorios
{
    public static readonly IReadOnlyList<(string Direito, TipoDocumentoEnum Tipo)> Itens =
    [
        // ---------- Autismo ----------
        ("CIPTEA", TipoDocumentoEnum.Laudo),
        ("CIPTEA", TipoDocumentoEnum.Rg),
        ("CIPTEA", TipoDocumentoEnum.Cpf),

        ("ACOMPANHANTE_ESCOLAR_TEA", TipoDocumentoEnum.Laudo),

        // ---------- Pessoa com deficiência ----------
        ("BPC_PCD", TipoDocumentoEnum.Laudo),
        ("BPC_PCD", TipoDocumentoEnum.Rg),
        ("BPC_PCD", TipoDocumentoEnum.Cpf),
        ("BPC_PCD", TipoDocumentoEnum.ComprovanteRenda),
        ("BPC_PCD", TipoDocumentoEnum.ComprovanteResidencia),

        ("PASSE_LIVRE_PCD", TipoDocumentoEnum.Laudo),
        ("PASSE_LIVRE_PCD", TipoDocumentoEnum.Rg),
        ("PASSE_LIVRE_PCD", TipoDocumentoEnum.Cpf),
        ("PASSE_LIVRE_PCD", TipoDocumentoEnum.ComprovanteRenda),

        ("ISENCAO_IPI_VEICULO_PCD", TipoDocumentoEnum.Laudo),
        ("ISENCAO_IPI_VEICULO_PCD", TipoDocumentoEnum.Rg),
        ("ISENCAO_IPI_VEICULO_PCD", TipoDocumentoEnum.Cpf),

        ("ISENCAO_IOF_VEICULO_PCD", TipoDocumentoEnum.Laudo),
        ("ISENCAO_IOF_VEICULO_PCD", TipoDocumentoEnum.Rg),
        ("ISENCAO_IOF_VEICULO_PCD", TipoDocumentoEnum.Cpf),

        ("COTA_PCD_ENSINO_SUPERIOR", TipoDocumentoEnum.Laudo),
        ("COTA_PCD_ENSINO_SUPERIOR", TipoDocumentoEnum.Rg),
        ("COTA_PCD_ENSINO_SUPERIOR", TipoDocumentoEnum.Cpf),

        ("COTA_PCD_CONCURSO", TipoDocumentoEnum.Laudo),
        ("COTA_PCD_CONCURSO", TipoDocumentoEnum.Rg),
        ("COTA_PCD_CONCURSO", TipoDocumentoEnum.Cpf),

        ("COTA_PCD_EMPREGO", TipoDocumentoEnum.Laudo),
        ("COTA_PCD_EMPREGO", TipoDocumentoEnum.Rg),
        ("COTA_PCD_EMPREGO", TipoDocumentoEnum.Cpf),

        ("PRIORIDADE_PCD", TipoDocumentoEnum.Laudo),
        ("PRIORIDADE_PCD", TipoDocumentoEnum.Rg),

        ("MEIA_ENTRADA_PCD", TipoDocumentoEnum.Laudo),
        ("MEIA_ENTRADA_PCD", TipoDocumentoEnum.Rg),

        ("CREDENCIAL_PCD", TipoDocumentoEnum.Laudo),
        ("CREDENCIAL_PCD", TipoDocumentoEnum.Rg),
        ("CREDENCIAL_PCD", TipoDocumentoEnum.ComprovanteResidencia),

        // ---------- Pessoa idosa ----------
        ("BPC_IDOSO", TipoDocumentoEnum.Rg),
        ("BPC_IDOSO", TipoDocumentoEnum.Cpf),
        ("BPC_IDOSO", TipoDocumentoEnum.ComprovanteRenda),
        ("BPC_IDOSO", TipoDocumentoEnum.ComprovanteResidencia),

        ("PASSE_IDOSO_INTERESTADUAL", TipoDocumentoEnum.Rg),
        ("PASSE_IDOSO_INTERESTADUAL", TipoDocumentoEnum.ComprovanteRenda),

        ("PRIORIDADE_IDOSO", TipoDocumentoEnum.Rg),

        ("ISENCAO_IR_65MAIS", TipoDocumentoEnum.Rg),
        ("ISENCAO_IR_65MAIS", TipoDocumentoEnum.Cpf),

        ("CREDENCIAL_IDOSO", TipoDocumentoEnum.Rg),
        ("CREDENCIAL_IDOSO", TipoDocumentoEnum.ComprovanteResidencia),

        // ---------- Baixa renda ----------
        ("CADUNICO", TipoDocumentoEnum.Rg),
        ("CADUNICO", TipoDocumentoEnum.Cpf),
        ("CADUNICO", TipoDocumentoEnum.ComprovanteRenda),
        ("CADUNICO", TipoDocumentoEnum.ComprovanteResidencia),

        ("BOLSA_FAMILIA", TipoDocumentoEnum.Rg),
        ("BOLSA_FAMILIA", TipoDocumentoEnum.Cpf),
        ("BOLSA_FAMILIA", TipoDocumentoEnum.ComprovanteRenda),
        ("BOLSA_FAMILIA", TipoDocumentoEnum.ComprovanteResidencia),

        ("TARIFA_SOCIAL_ENERGIA", TipoDocumentoEnum.Cpf),
        ("TARIFA_SOCIAL_ENERGIA", TipoDocumentoEnum.ComprovanteResidencia)
    ];
}
