using DireitoFacil.Application.DTOs;

namespace DireitoFacil.Application.Services.Interfaces;

/// <summary>
/// Casos de uso de Documento.
/// </summary>
public interface IDocumentoService
{
    /// <summary>
    /// Valida o arquivo, grava no storage, registra o documento e agenda o evento DocumentoRecebido.
    /// </summary>
    Task<DocumentoResponse> CriarAsync(
        CriarDocumentoRequest request, ArquivoUpload arquivo, CancellationToken cancellationToken = default);

    Task<PagedResponse<DocumentoResponse>> ListarAsync(DocumentoFiltro filtro, CancellationToken cancellationToken = default);

    Task<DocumentoResponse> ObterAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ArquivoResponse> AbrirArquivoAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DocumentoResponse> AtualizarAsync(
        Guid id, AtualizarDocumentoRequest request, CancellationToken cancellationToken = default);

    /// <summary>Usado pela API Java (papel SERVICO) para informar o resultado da extração.</summary>
    Task AtualizarStatusAsync(
        Guid id, AtualizarStatusDocumentoRequest request, CancellationToken cancellationToken = default);

    /// <summary>Exclui o registro e o arquivo (direito de exclusão, LGPD art. 18).</summary>
    Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default);
}
