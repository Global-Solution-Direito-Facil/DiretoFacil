using DireitoFacil.Api.Health;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DireitoFacil.Api.Extensions;

public static class HealthCheckServiceCollectionExtensions
{
    /// <summary>
    /// self (processo), oracle (banco), storage (arquivos), rabbitmq (fila) e api-java (serviço externo).
    /// </summary>
    public static IServiceCollection AddDireitoFacilHealthChecks(this IServiceCollection services)
    {
        services.AddHttpClient(nameof(ServicoExternoHealthCheck));

        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("O processo da API está no ar e funcional."), tags: ["live"])
            .AddCheck<OracleHealthCheck>("oracle", tags: ["ready", "db"])
            .AddCheck<StorageHealthCheck>("storage", tags: ["ready"])
            .AddCheck<RabbitMqHealthCheck>("rabbitmq", tags: ["ready", "external"])
            .AddCheck<ServicoExternoHealthCheck>("api-java", tags: ["ready", "external"]);

        return services;
    }
}
