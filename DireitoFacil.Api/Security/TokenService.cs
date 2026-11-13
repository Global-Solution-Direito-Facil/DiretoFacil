using System.Security.Claims;
using System.Text;
using DireitoFacil.Application.Abstractions;
using DireitoFacil.Application.DTOs;
using DiretoFacil.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace DireitoFacil.Api.Security;

/// <summary>
/// Emite o JWT assinado com HMAC-SHA256. Claims: sub, email, name, role e jti.
/// </summary>
public sealed class TokenService(IOptions<JwtOptions> options) : ITokenService
{
    private readonly JwtOptions _options = options.Value;
    private readonly JsonWebTokenHandler _handler = new();

    public TokenResponse Gerar(Usuario usuario)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        var agora = DateTime.UtcNow;
        var expiraEm = agora.AddMinutes(_options.ExpiracaoMinutos);
        var subject = _options.SubjectEhEmail ? usuario.Email : usuario.Id.ToString();

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = agora,
            NotBefore = agora,
            Expires = expiraEm,
            Subject = new ClaimsIdentity(new List<Claim>
            {
                new("sub", subject),
                new("email", usuario.Email),
                new("name", usuario.Nome),
                new("jti", Guid.NewGuid().ToString()),
                new("role", Papeis.DoEnum(usuario.Papel))
            }),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Chave)),
                SecurityAlgorithms.HmacSha256)
        };

        var token = _handler.CreateToken(descriptor);
        return new TokenResponse(token, expiraEm, UsuarioResponse.FromDomain(usuario));
    }
}
