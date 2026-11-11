using DireitoFacil.Application.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace DireitoFacil.Infrastructure.Security;

/// <summary>
/// Hash de senha com o PasswordHasher do ASP.NET Core Identity (PBKDF2 com HMAC-SHA512,
/// salt aleatório e 100 mil iterações). O hash já carrega o salt e a versão do algoritmo.
/// </summary>
public sealed class SenhaHasher : ISenhaHasher
{
    private static readonly object Usuario = new();
    private readonly PasswordHasher<object> _hasher = new();

    public string GerarHash(string senha)
    {
        ArgumentException.ThrowIfNullOrEmpty(senha);
        return _hasher.HashPassword(Usuario, senha);
    }

    public bool Verificar(string senhaHash, string senha)
    {
        if (string.IsNullOrEmpty(senhaHash) || string.IsNullOrEmpty(senha))
            return false;

        try
        {
            return _hasher.VerifyHashedPassword(Usuario, senhaHash, senha) != PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            // Hash corrompido ou em formato desconhecido: trata como senha inválida.
            return false;
        }
    }
}
