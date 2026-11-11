using DireitoFacil.Infrastructure.Messaging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace DireitoFacil.Api.Health;

/// <summary>
/// Verifica a conexão com o RabbitMQ. Falha aqui é Degraded, não Unhealthy: graças ao
/// outbox, a API continua aceitando documentos e os eventos são publicados quando a fila voltar.
/// </summary>
public sealed class RabbitMqHealthCheck(RabbitMqPublisher publisher, IOptions<RabbitMqOptions> options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!options.Value.Configurado)
            return HealthCheckResult.Degraded("RabbitMQ não configurado: eventos acumulando no outbox.");

        try
        {
            return await publisher.EstaConectavelAsync(cancellationToken)
                ? HealthCheckResult.Healthy("RabbitMQ acessível.")
                : HealthCheckResult.Degraded("Conexão com o RabbitMQ fechada.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded("RabbitMQ indisponível: eventos acumulando no outbox.", ex);
        }
    }
}
