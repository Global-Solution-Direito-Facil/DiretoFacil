using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DireitoFacil.Api.Health;

/// <summary>
/// Serializa o <see cref="HealthReport"/> em JSON: status geral, duração e cada check.
/// O detalhe da exceção só aparece em Development, para não vazar informação em produção.
/// </summary>
public static class HealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions Opcoes = new() { WriteIndented = true };

    public static Task WriteJsonResponse(HttpContext context, HealthReport report)
    {
        var environment = context.RequestServices.GetRequiredService<IHostEnvironment>();

        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                durationMs = e.Value.Duration.TotalMilliseconds,
                error = environment.IsDevelopment() ? e.Value.Exception?.Message : null
            })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload, Opcoes));
    }
}
