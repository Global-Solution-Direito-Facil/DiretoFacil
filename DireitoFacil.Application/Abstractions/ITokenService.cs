using DireitoFacil.Application.DTOs;
using DiretoFacil.Domain.Entities;

namespace DireitoFacil.Application.Abstractions;

/// <summary>
/// Emite o JWT do usuário autenticado. A implementação fica na Api, que conhece a chave,
/// o issuer, a audience e quais claims vão no token ("sub", "role", "email").
/// </summary>
public interface ITokenService
{
    TokenResponse Gerar(Usuario usuario);
}
