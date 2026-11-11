using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DireitoFacil.Api.Health;

/// <summary>
/// Verifica a API Java (serviço de IA) pela URL configurada em ServicosExternos:JavaApiHealthUrl.
/// É Degraded em caso de falha: sem a IA, os documentos ficam em RECEBIDO, mas a API segue funcionando.
/// </summary>
public sealed class ServicoExternoHealthCheck(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IHealthCheck
{
    public const string Chave = "ServicosExternos:JavaApiHealthUrl";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var url = configuration[Chave];
        if (string.IsNullOrWhiteSpace(url))
            return HealthCheckResult.Healthy("API Java não configurada para monitoramento.");

        try
        {
            var client = httpClientFactory.CreateClient(nameof(ServicoExternoHealthCheck));
            client.Timeout = TimeSpan.FromSeconds(5);

            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy($"API Java respondeu {(int)response.StatusCode}.")
                : HealthCheckResult.Degraded($"API Java respondeu {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded("Não foi possível alcançar a API Java.", ex);
        }
    }
}
