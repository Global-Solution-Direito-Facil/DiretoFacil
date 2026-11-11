namespace DireitoFacil.Application.Abstractions;

/// <summary>
/// Gera e confere hash de senha. A implementação (PBKDF2 via PasswordHasher do ASP.NET)
/// fica na Infrastructure; a senha em texto nunca é gravada.
/// </summary>
public interface ISenhaHasher
{
    string GerarHash(string senha);

    bool Verificar(string senhaHash, string senha);
}
