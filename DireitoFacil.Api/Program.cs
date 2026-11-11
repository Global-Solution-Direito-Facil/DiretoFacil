using System.Text.Json.Serialization;
using DireitoFacil.Api;
using DireitoFacil.Api.Exceptions;
using DireitoFacil.Api.Extensions;
using DireitoFacil.Api.Health;
using DireitoFacil.Infrastructure.Persistence;
using DireitoFacil.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDireitoFacilDbContext(builder.Configuration, builder.Environment);
builder.Services.AddDireitoFacilRepositories();
builder.Services.AddDireitoFacilApplicationServices();
builder.Services.AddDireitoFacilInfraestrutura(builder.Configuration);
builder.Services.AddDireitoFacilAutenticacao(builder.Configuration);
builder.Services.AddDireitoFacilRateLimiting();
builder.Services.AddDireitoFacilHealthChecks();

// Qualquer exceção não tratada vira application/problem+json (GlobalExceptionHandler).
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// No Azure App Service a requisição chega por um proxy: o IP real vem em X-Forwarded-For
// (necessário para o rate limiting por IP funcionar).
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDireitoFacilSwagger(builder.Configuration);

var app = builder.Build();

if (app.Configuration.GetValue("Database:AplicarMigrations", true))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<DireitoFacilContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync();
}

app.UseForwardedHeaders();

// Registrado cedo para cobrir exceções de todos os middlewares seguintes.
app.UseExceptionHandler();

// Swagger habilitado também na nuvem, para a demonstração e a correção.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "DireitoFácil API v1");
    options.RoutePrefix = string.Empty;

    // Facilidade só da tela de testes: se o campo Idempotency-Key ficar vazio num POST,
    // o Swagger gera um UUID novo sozinho. Para testar a repetição, digite a mesma chave duas vezes.
    // A API continua exigindo o header: chamadas de fora do Swagger sem ele recebem 400.
    options.UseRequestInterceptor(SwaggerIdempotencia.Interceptor);
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Depois da autenticação: as políticas por usuário leem a claim "sub".
app.UseRateLimiter();

app.MapControllers();

// /health: tudo (processo, Oracle, storage, RabbitMQ, API Java). Healthy/Degraded → 200; Unhealthy → 503.
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthCheckResponseWriter.WriteJsonResponse,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});

// /health/live: só o processo (para o Azure saber se precisa reiniciar o app).
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = HealthCheckResponseWriter.WriteJsonResponse
});

app.Run();

/// <summary>Exposto para os testes de integração (WebApplicationFactory).</summary>
public partial class Program
{
}
