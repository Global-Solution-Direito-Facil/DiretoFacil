namespace DireitoFacil.Api.Security;

/// <summary>
/// Seção "Jwt" do appsettings. A chave NUNCA fica no código nem no appsettings versionado:
/// vem de user-secrets (local) ou variável de ambiente Jwt__Chave (Azure).
/// Para o app fazer um único login, Java e .NET precisam usar a mesma Chave, Issuer e Audience.
/// </summary>
public sealed class JwtOptions
{
    public const string Secao = "Jwt";

    public string Issuer { get; set; } = "direitofacil";
    public string Audience { get; set; } = "direitofacil-app";

    /// <summary>Chave HMAC-SHA256 com pelo menos 32 caracteres.</summary>
    public string Chave { get; set; } = string.Empty;

    public int ExpiracaoMinutos { get; set; } = 480;

    /// <summary>
    /// O que vai na claim "sub": "Email" (padrão, identificador comum com a API Java) ou "Id".
    /// </summary>
    public string SubjectClaim { get; set; } = "Email";

    public bool SubjectEhEmail => !string.Equals(SubjectClaim, "Id", StringComparison.OrdinalIgnoreCase);

    public void Validar()
    {
        if (string.IsNullOrWhiteSpace(Chave) || Chave.Length < 32)
            throw new InvalidOperationException(
                "Jwt:Chave ausente ou com menos de 32 caracteres. Configure com " +
                "'dotnet user-secrets set \"Jwt:Chave\" \"...\"' (local) ou a variável de ambiente Jwt__Chave (Azure).");
    }
}
