using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using DireitoFacil.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace DireitoFacil.Infrastructure.Storage;

/// <summary>
/// Storage no Azure Blob, em container privado (sem acesso público). Leituras externas
/// (serviço de IA) usam URL SAS temporária, somente leitura.
/// </summary>
public sealed class BlobArquivoStorage : IArquivoStorage
{
    private readonly BlobContainerClient _container;

    public BlobArquivoStorage(IOptions<StorageOptions> options)
    {
        var config = options.Value;
        if (string.IsNullOrWhiteSpace(config.AzureConnectionString))
            throw new InvalidOperationException("Storage:AzureConnectionString não configurada.");

        _container = new BlobContainerClient(config.AzureConnectionString, config.Container);
        _container.CreateIfNotExists(PublicAccessType.None);
    }

    public async Task SalvarAsync(string caminho, Stream conteudo, string contentType, CancellationToken cancellationToken = default)
    {
        var blob = _container.GetBlobClient(caminho);
        await blob.UploadAsync(
            conteudo,
            new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } },
            cancellationToken);
    }

    public async Task<Stream> AbrirAsync(string caminho, CancellationToken cancellationToken = default)
    {
        var blob = _container.GetBlobClient(caminho);
        if (!(await blob.ExistsAsync(cancellationToken)).Value)
            throw new KeyNotFoundException("Arquivo não encontrado no storage.");

        return await blob.OpenReadAsync(cancellationToken: cancellationToken);
    }

    public Task ExcluirAsync(string caminho, CancellationToken cancellationToken = default)
        => _container.GetBlobClient(caminho).DeleteIfExistsAsync(cancellationToken: cancellationToken);

    public Uri GerarUrlLeitura(string caminho, TimeSpan validade)
    {
        var blob = _container.GetBlobClient(caminho);
        if (!blob.CanGenerateSasUri)
            throw new InvalidOperationException("A connection string do Storage precisa conter a AccountKey para gerar SAS.");

        return blob.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(validade));
    }

    /// <summary>Usado pelo health check: confirma que o container responde.</summary>
    public async Task<bool> ContainerAcessivelAsync(CancellationToken cancellationToken)
        => (await _container.ExistsAsync(cancellationToken)).Value;
}
