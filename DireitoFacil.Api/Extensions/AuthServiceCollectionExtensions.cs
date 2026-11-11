using System.Text;
using DireitoFacil.Api.Security;
using DireitoFacil.Application.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace DireitoFacil.Api.Extensions;

public static class AuthServiceCollectionExtensions
{
    /// <summary>
    /// JWT (HS256) com papéis CIDADAO, GESTOR e SERVICO. O mesmo Issuer/Audience/Chave
    /// aceita tokens emitidos pela API Java, se o grupo configurar os mesmos valores.
    /// </summary>
    public static IServiceCollection AddDireitoFacilAutenticacao(
        this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.Secao).Get<JwtOptions>() ?? new JwtOptions();
        jwt.Validar();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.Secao));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Mantém os nomes originais das claims ("sub", "role") em vez de URIs longas do .NET.
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Chave)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "sub",
                    RoleClaimType = "role"
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.SomenteCidadao, p => p.RequireRole(Papeis.Cidadao))
            .AddPolicy(Policies.SomenteGestor, p => p.RequireRole(Papeis.Gestor))
            .AddPolicy(Policies.SomenteServico, p => p.RequireRole(Papeis.Servico));

        services.AddHttpContextAccessor();
        services.AddScoped<IUsuarioAtual, UsuarioAtual>();
        services.AddSingleton<ITokenService, TokenService>();

        return services;
    }
}
