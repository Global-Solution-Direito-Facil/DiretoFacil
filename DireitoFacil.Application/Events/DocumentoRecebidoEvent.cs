namespace DireitoFacil.Application.Events;

/// <summary>
/// Contrato do evento consumido pela API Java (Spring AI) para extrair os dados do laudo.
/// Não carrega nome, CPF nem conteúdo do arquivo (LGPD): só identificadores e metadados.
/// A URL de leitura (SAS) é gerada pelo OutboxWorker no momento da publicação.
/// </summary>
public record DocumentoRecebidoEvent(
    Guid EventId,
    Guid DocumentoId,
    string CidadaoId,
    string TipoDocumento,
    string ContentType,
    string BlobPath,
    DateTime CriadoEm);
