using DireitoFacil.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace DireitoFacil.Infrastructure.Storage;

/// <summary>
/// Storage em disco para desenvolvimento e testes: dispensa conta no Azure para rodar a API.
/// </summary>
public sealed class LocalArquivoStorage(IOptions<StorageOptions> options) : IArquivoStorage
{
    private readonly string _raiz = Path.GetFullPath(options.Value.PastaLocal);

    public async Task SalvarAsync(string caminho, Stream conteudo, string contentType, CancellationToken cancellationToken = default)
    {
        var arquivo = Resolver(caminho);
        Directory.CreateDirectory(Path.GetDirectoryName(arquivo)!);

        await using var destino = File.Create(arquivo);
        await conteudo.CopyToAsync(destino, cancellationToken);
    }

    public Task<Stream> AbrirAsync(string caminho, CancellationToken cancellationToken = default)
    {
        var arquivo = Resolver(caminho);
        if (!File.Exists(arquivo))
            throw new KeyNotFoundException("Arquivo não encontrado no storage.");

        return Task.FromResult<Stream>(File.OpenRead(arquivo));
    }

    public Task ExcluirAsync(string caminho, CancellationToken cancellationToken = default)
    {
        var arquivo = Resolver(caminho);
        if (File.Exists(arquivo))
            File.Delete(arquivo);

        return Task.CompletedTask;
    }

    public Uri GerarUrlLeitura(string caminho, TimeSpan validade) => new(Resolver(caminho));

    /// <summary>Impede que um caminho com ".." saia da pasta de arquivos.</summary>
    private string Resolver(string caminho)
    {
        var completo = Path.GetFullPath(Path.Combine(_raiz, caminho));
        if (!completo.StartsWith(_raiz + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new ArgumentException("Caminho de arquivo inválido.", nameof(caminho));

        return completo;
    }
}
