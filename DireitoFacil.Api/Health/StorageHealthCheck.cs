using System.Text;
using DireitoFacil.Application.Abstractions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DireitoFacil.Api.Health;

/// <summary>
/// Grava e apaga um arquivo de teste: confirma que o storage dos documentos aceita escrita.
/// Sem storage não há upload, então a falha deixa a API Unhealthy.
/// </summary>
public sealed class StorageHealthCheck(IArquivoStorage storage) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var caminho = $"_health/{Guid.NewGuid():N}.txt";
        try
        {
            using var conteudo = new MemoryStream(Encoding.UTF8.GetBytes("ok"));
            await storage.SalvarAsync(caminho, conteudo, "text/plain", cancellationToken);
            await storage.ExcluirAsync(caminho, cancellationToken);

            return HealthCheckResult.Healthy($"Storage de documentos ({storage.GetType().Name}) aceita gravação.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Storage de documentos indisponível.", ex);
        }
    }
}
