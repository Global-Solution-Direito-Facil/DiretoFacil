namespace DireitoFacil.Infrastructure.Storage;

/// <summary>
/// Seção "Storage" do appsettings. Provider "Local" grava em disco (desenvolvimento);
/// "AzureBlob" usa um container privado no Azure Storage (nuvem).
/// </summary>
public sealed class StorageOptions
{
    public const string Secao = "Storage";

    public string Provider { get; set; } = "Local";

    /// <summary>Pasta usada pelo provider Local.</summary>
    public string PastaLocal { get; set; } = "arquivos";

    /// <summary>Connection string do Azure Storage (via variável de ambiente, nunca no código).</summary>
    public string? AzureConnectionString { get; set; }

    public string Container { get; set; } = "documentos";

    public bool UsaAzureBlob => string.Equals(Provider, "AzureBlob", StringComparison.OrdinalIgnoreCase);
}
