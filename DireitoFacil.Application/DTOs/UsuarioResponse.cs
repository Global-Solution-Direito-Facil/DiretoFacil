using DireitoFacil.Domain.Entities;
using DireitoFacil.Domain.Enums;

namespace DireitoFacil.Application.DTOs;

/// <summary>
/// Dados públicos do usuário. Nunca expõe o hash da senha.
/// </summary>
public record UsuarioResponse(Guid Id, string Nome, string Email, PapelUsuarioEnum Papel)
{
    public static UsuarioResponse FromDomain(Usuario usuario) =>
        new(usuario.Id, usuario.Nome, usuario.Email, usuario.Papel);
}
