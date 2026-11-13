using System.Threading.RateLimiting;

namespace DireitoFacil.Api.Extensions;

/// <summary>
/// Nomes das políticas de rate limiting usadas em [EnableRateLimiting].
/// </summary>
public static class RateLimitPolicies
{
    /// <summary>Login e cadastro: 10 por minuto por IP (contra força bruta).</summary>
    public const string Autenticacao = "autenticacao";

    /// <summary>Upload: 5 em sequência, recarrega 1 a cada 2 minutos por usuário (cada upload aciona a IA).</summary>
    public const string Upload = "upload";

    /// <summary>Demais escritas: 30 por minuto por usuário.</summary>
    public const string Escrita = "escrita";

    /// <summary>Leituras: 120 por minuto por usuário.</summary>
    public const string Leitura = "leitura";
}

public static class RateLimitServiceCollectionExtensions
{
    public static IServiceCollection AddDireitoFacilRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(RateLimitPolicies.Autenticacao, http =>
                RateLimitPartition.GetFixedWindowLimiter(
                    PorIp(http),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.AddPolicy(RateLimitPolicies.Upload, http =>
                RateLimitPartition.GetTokenBucketLimiter(
                    PorUsuario(http),
                    _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 5,
                        TokensPerPeriod = 1,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(2),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.AddPolicy(RateLimitPolicies.Escrita, http =>
                RateLimitPartition.GetFixedWindowLimiter(
                    PorUsuario(http),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.AddPolicy(RateLimitPolicies.Leitura, http =>
                RateLimitPartition.GetFixedWindowLimiter(
                    PorUsuario(http),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 120,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.OnRejected = async (context, cancellationToken) =>
            {
                var retryAfterSeconds = 60;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    retryAfterSeconds = (int)Math.Ceiling(retryAfter.TotalSeconds);

                context.HttpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString();
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                // O contentType precisa ir no próprio WriteAsJsonAsync: senão ele sobrescreve com application/json.
                await context.HttpContext.Response.WriteAsJsonAsync(
                    new
                    {
                        type = "about:blank",
                        title = "Limite de requisições excedido",
                        status = StatusCodes.Status429TooManyRequests,
                        detail = $"Você excedeu o limite de requisições para este endpoint. " +
                                 $"Tente novamente em {retryAfterSeconds} segundo(s).",
                        instance = context.HttpContext.Request.Path.Value
                    },
                    options: null,
                    contentType: "application/problem+json",
                    cancellationToken: cancellationToken);
            };
        });

        return services;
    }

    private static string PorIp(HttpContext http)
        => "ip:" + (http.Connection.RemoteIpAddress?.ToString() ?? "desconhecido");

    /// <summary>Usuário autenticado (claim sub); sem login, cai no IP.</summary>
    private static string PorUsuario(HttpContext http)
        => http.User.FindFirst("sub")?.Value is { } sub ? "usuario:" + sub : PorIp(http);
}
