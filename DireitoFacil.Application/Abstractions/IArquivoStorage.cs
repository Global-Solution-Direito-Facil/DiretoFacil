namespace DireitoFacil.Application.Abstractions;

/// <summary>
/// Armazenamento privado dos arquivos enviados (Azure Blob Storage na Infrastructure).
/// </summary>
public interface IArquivoStorage
{
    Task SalvarAsync(string caminho, Stream conteudo, string contentType, CancellationToken cancellationToken = default);

    Task<Stream> AbrirAsync(string caminho, CancellationToken cancellationToken = default);

    Task ExcluirAsync(string caminho, CancellationToken cancellationToken = default);

    /// <summary>
    /// URL de leitura temporária (SAS), enviada no evento para o serviço de IA baixar o arquivo.
    /// </summary>
    Uri GerarUrlLeitura(string caminho, TimeSpan validade);
}
