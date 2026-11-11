using DireitoFacil.Infrastructure.Storage;
using Microsoft.Extensions.Options;

namespace DireitoFacil.Infrastructure.Tests.Storage;

public class LocalArquivoStorageTests : IDisposable
{
    private readonly string _pasta = Path.Combine(Path.GetTempPath(), "direitofacil-testes", Guid.NewGuid().ToString("N"));
    private readonly LocalArquivoStorage _storage;

    public LocalArquivoStorageTests()
        => _storage = new LocalArquivoStorage(Options.Create(new StorageOptions { PastaLocal = _pasta }));

    public void Dispose()
    {
        if (Directory.Exists(_pasta))
            Directory.Delete(_pasta, recursive: true);
    }

    [Fact]
    public async Task SalvarEAbrir_DeveDevolverOMesmoConteudo()
    {
        // Arrange
        byte[] conteudo = [0x25, 0x50, 0x44, 0x46, 0x01, 0x02];

        // Act
        await _storage.SalvarAsync("cidadao/doc1", new MemoryStream(conteudo), "application/pdf");
        await using var lido = await _storage.AbrirAsync("cidadao/doc1");
        using var copia = new MemoryStream();
        await lido.CopyToAsync(copia);

        // Assert
        Assert.Equal(conteudo, copia.ToArray());
    }

    [Fact]
    public async Task Excluir_DepoisAbrir_DeveLancarKeyNotFoundException()
    {
        // Arrange
        await _storage.SalvarAsync("cidadao/doc2", new MemoryStream([1, 2, 3]), "application/pdf");

        // Act
        await _storage.ExcluirAsync("cidadao/doc2");

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _storage.AbrirAsync("cidadao/doc2"));
    }

    [Fact]
    public async Task Excluir_ArquivoInexistente_NaoDeveLancar()
    {
        // Act
        var ex = await Record.ExceptionAsync(() => _storage.ExcluirAsync("nao/existe"));

        // Assert
        Assert.Null(ex);
    }

    [Theory]
    [InlineData("../fora.txt")]
    [InlineData("../../etc/passwd")]
    public async Task Salvar_CaminhoSaindoDaPasta_DeveLancarArgumentException(string caminho)
    {
        // Act + Assert: impede path traversal
        await Assert.ThrowsAsync<ArgumentException>(
            () => _storage.SalvarAsync(caminho, new MemoryStream([1]), "text/plain"));
    }
}
