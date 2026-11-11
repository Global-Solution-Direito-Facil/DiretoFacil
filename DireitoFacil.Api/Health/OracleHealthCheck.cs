using DireitoFacil.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DireitoFacil.Api.Health;

/// <summary>
/// Verifica se o banco Oracle está acessível.
/// </summary>
public sealed class OracleHealthCheck(DireitoFacilContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("Banco Oracle acessível.")
                : HealthCheckResult.Unhealthy("Não foi possível conectar ao banco Oracle.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Falha ao conectar ao banco Oracle.", ex);
        }
    }
}
