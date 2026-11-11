using System.Net;
using System.Text.Json;
using DireitoFacil.Api.Tests.Infra;

namespace DireitoFacil.Api.Tests.Endpoints;

public class HealthTests(DireitoFacilApiFactory factory) : IClassFixture<DireitoFacilApiFactory>
{
    [Fact]
    public async Task Health_DeveListarBancoStorageFilaEApiJava()
    {
        // Act
        var resposta = await factory.CreateClient().GetAsync("/health");

        // Assert: RabbitMQ não configurado deixa Degraded, que ainda responde 200
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
        var checks = json.RootElement.GetProperty("checks").EnumerateArray()
            .ToDictionary(c => c.GetProperty("name").GetString()!, c => c.GetProperty("status").GetString());

        Assert.Equal("Healthy", checks["self"]);
        Assert.Equal("Healthy", checks["oracle"]);
        Assert.Equal("Healthy", checks["storage"]);
        Assert.Equal("Degraded", checks["rabbitmq"]);
        Assert.True(checks.ContainsKey("api-java"));
    }

    [Fact]
    public async Task HealthLive_DeveResponderSemDependencias()
    {
        // Act
        var resposta = await factory.CreateClient().GetAsync("/health/live");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task Swagger_DevePublicarODocumentoOpenApi()
    {
        // Act
        var resposta = await factory.CreateClient().GetAsync("/swagger/v1/swagger.json");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Contains("/api/documentos", await resposta.Content.ReadAsStringAsync());
    }
}
