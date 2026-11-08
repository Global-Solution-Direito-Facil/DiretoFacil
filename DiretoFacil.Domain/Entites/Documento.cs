
using DiretoFacil.Domain.Common;
using DiretoFacil.Domain.Enums;
using DiretoFacil.Domain.Expections;

namespace AbrePortas.Domain.Entities;

/// <summary>
/// Documento enviado pela família (laudo, receita, comprovantes).
/// O arquivo fica no Blob Storage; aqui ficam só os metadados.
/// </summary>
public class Documento : BaseEntity
{
    public const long TamanhoMaximoArquivo = 10 * 1024 * 1024; // 10 MB

    // Tipos de arquivo permitidos: PDF, JPEG e PNG.
    public static readonly IReadOnlySet<string> ContentTypesPermitidos =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "application/pdf", "image/jpeg", "image/png" };

    public string CidadaoId { get; private set; } = null!;
    public TipoDocumentoEnum Tipo { get; private set; }
    public string? Descricao { get; private set; }
    public string NomeArquivo { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long TamanhoArquivo { get; private set; }
    public string BlobPath { get; private set; } = null!;
    public string HashSha256 { get; private set; } = null!;
    public StatusDocumentoEnum Status { get; private set; }
    public bool ConsentimentoSaude { get; private set; }

    protected Documento()
    {
    }

    public Documento(
        string cidadaoId,
        TipoDocumentoEnum tipo,
        string nomeArquivo,
        string contentType,
        long tamanhoArquivo,
        string hashSha256,
        bool consentimentoSaude,
        string? descricao = null)
    {
        if (string.IsNullOrWhiteSpace(cidadaoId))
            throw new DomainException("O cidadão é obrigatório.");

        if (string.IsNullOrWhiteSpace(nomeArquivo))
            throw new DomainException("O nome do arquivo é obrigatório.");

        if (string.IsNullOrWhiteSpace(contentType) || !ContentTypesPermitidos.Contains(contentType))
            throw new DomainException("Formato não permitido. Envie PDF, JPEG ou PNG.");

        if (tamanhoArquivo <= 0 || tamanhoArquivo > TamanhoMaximoArquivo)
            throw new DomainException("O arquivo deve ter entre 1 byte e 10 MB.");

        if (string.IsNullOrWhiteSpace(hashSha256))
            throw new DomainException("O hash do arquivo é obrigatório.");

        ValidarConsentimento(tipo, consentimentoSaude);

        CidadaoId = cidadaoId;
        Tipo = tipo;
        NomeArquivo = nomeArquivo.Trim();
        ContentType = contentType.ToLowerInvariant();
        TamanhoArquivo = tamanhoArquivo;
        HashSha256 = hashSha256.ToUpperInvariant();
        ConsentimentoSaude = consentimentoSaude;
        Descricao = descricao?.Trim();
        BlobPath = $"{cidadaoId}/{Id:N}";
        Status = StatusDocumentoEnum.Recebido;
    }

    /// <summary>
    /// Corrige o tipo e a descrição informados pela família.
    /// Bloqueado enquanto a IA está processando o arquivo.
    /// </summary>
    public void Reclassificar(TipoDocumentoEnum tipo, string? descricao, bool consentimentoSaude)
    {
        if (Status == StatusDocumentoEnum.Processando)
            throw new RegraDeNegocioException("O documento está sendo processado. Tente novamente em instantes.");

        var consentimento = ConsentimentoSaude || consentimentoSaude;
        ValidarConsentimento(tipo, consentimento);

        Tipo = tipo;
        Descricao = descricao?.Trim();
        ConsentimentoSaude = consentimento;
        MarcarAtualizado();
    }

    public void IniciarProcessamento()
    {
        if (Status is not (StatusDocumentoEnum.Recebido or StatusDocumentoEnum.Falha))
            throw new RegraDeNegocioException($"Não é possível processar um documento {Status}.");

        Status = StatusDocumentoEnum.Processando;
        MarcarAtualizado();
    }

    /// <summary>
    /// Idempotente: se o Java reenviar o mesmo resultado, não gera erro.
    /// </summary>
    public void MarcarExtraido()
    {
        if (Status == StatusDocumentoEnum.Extraido)
            return;

        if (Status is not (StatusDocumentoEnum.Recebido or StatusDocumentoEnum.Processando))
            throw new RegraDeNegocioException($"Não é possível marcar como extraído um documento {Status}.");

        Status = StatusDocumentoEnum.Extraido;
        MarcarAtualizado();
    }

    public void MarcarFalha()
    {
        if (Status == StatusDocumentoEnum.Falha)
            return;

        if (Status is not (StatusDocumentoEnum.Recebido or StatusDocumentoEnum.Processando))
            throw new RegraDeNegocioException($"Não é possível marcar como falha um documento {Status}.");

        Status = StatusDocumentoEnum.Falha;
        MarcarAtualizado();
    }

    /// <summary>
    /// Ponto único usado pelo PATCH /documentos/{id}/status (chamado pelo serviço Java).
    /// </summary>
    public void AtualizarStatus(StatusDocumentoEnum novoStatus)
    {
        switch (novoStatus)
        {
            case StatusDocumentoEnum.Processando: IniciarProcessamento(); break;
            case StatusDocumentoEnum.Extraido: MarcarExtraido(); break;
            case StatusDocumentoEnum.Falha: MarcarFalha(); break;
            default: throw new DomainException($"Status {novoStatus} não pode ser definido externamente.");
        }
    }

    public bool PertenceA(string cidadaoId) => CidadaoId == cidadaoId;

    private static void ValidarConsentimento(TipoDocumentoEnum tipo, bool consentimentoSaude)
    {
        if (tipo == TipoDocumentoEnum.Laudo && !consentimentoSaude)
            throw new DomainException(
                "Laudo é dado de saúde: é necessário o consentimento específico (LGPD, art. 11).");
    }
}