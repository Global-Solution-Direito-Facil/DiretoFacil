using DireitoFacil.Application.DTOs;

namespace DireitoFacil.Application.Services.Interfaces;

/// <summary>
/// Cadastro e login com JWT próprio.
/// </summary>
public interface IAuthService
{
    /// <summary>Cria uma conta de cidadão.</summary>
    Task<UsuarioResponse> RegistrarAsync(RegistrarUsuarioRequest request, CancellationToken cancellationToken = default);

    /// <summary>Confere as credenciais e devolve o JWT.</summary>
    Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
