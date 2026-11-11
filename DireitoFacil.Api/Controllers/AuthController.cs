using DireitoFacil.Application.DTOs;
using DireitoFacil.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DireitoFacil.Api.Controllers;

/// <summary>
/// Cadastro e login (JWT próprio). Endpoints públicos, com rate limiting mais rígido
/// para dificultar ataques de força bruta.
/// </summary>
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
[Produces("application/json")]
[EnableRateLimiting(RateLimitPolicies.Autenticacao)]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Cadastra um cidadão. Contas de assistente social não são criadas por aqui.
    /// </summary>
    /// <param name="request">Nome, e-mail e senha (8+ caracteres, com letras e números).</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpPost("registrar")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarUsuarioRequest request, CancellationToken cancellationToken)
    {
        var usuario = await authService.RegistrarAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, usuario);
    }

    /// <summary>
    /// Autentica e devolve o JWT. Use-o no header: Authorization: Bearer {accessToken}.
    /// </summary>
    /// <param name="request">E-mail e senha.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        => Ok(await authService.LoginAsync(request, cancellationToken));
}
