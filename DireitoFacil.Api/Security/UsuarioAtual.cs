using DireitoFacil.Application.Abstractions;

namespace DireitoFacil.Api.Security;

/// <summary>
/// Lê o usuário da requisição a partir das claims do JWT validado.
/// </summary>
public sealed class UsuarioAtual(IHttpContextAccessor accessor) : IUsuarioAtual
{
    private System.Security.Claims.ClaimsPrincipal Principal =>
        accessor.HttpContext?.User ?? throw new UnauthorizedAccessException("Requisição sem contexto HTTP.");

    public string Id =>
        Principal.FindFirst("sub")?.Value
        ?? throw new UnauthorizedAccessException("Token sem identificação do usuário.");

    public bool EhGestor => Principal.IsInRole(Papeis.Gestor);

    public bool EhServico => Principal.IsInRole(Papeis.Servico);
}
