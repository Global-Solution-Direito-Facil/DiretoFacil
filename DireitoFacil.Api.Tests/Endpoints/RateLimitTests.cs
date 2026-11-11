using System.Net;
using System.Net.Http.Json;
using DireitoFacil.Api.Tests.Infra;
using DireitoFacil.Application.DTOs;
using DiretoFacil.Domain.Enums;

namespace DireitoFacil.Api.Tests.Endpoints;

/// <summary>
/// Rate limiting. Cada teste usa a sua própria API (factory nova) para começar com o limite zerado.
/// </summary>
public class RateLimitTests
{
    [Fact]
    public async Task Login_AcimaDe10PorMinuto_DeveRetornar429ComRetryAfter()
    {
        // Arrange
        await using var factory = new DireitoFacilApiFactory();
        var client = factory.CreateClient();
        var login = new LoginRequest("ninguem@teste.com", "errada123");

        // Act: 10 tentativas permitidas (401) e a 11ª bloqueada
        for (var i = 0; i < 10; i++)
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/auth/login", login)).StatusCode);

        var bloqueada = await client.PostAsJsonAsync("/api/auth/login", login);

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, bloqueada.StatusCode);
        Assert.True(bloqueada.Headers.Contains("Retry-After"));
        Assert.Equal("application/problem+json", bloqueada.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Upload_SextoEnvioSeguido_DeveRetornar429()
    {
        // Arrange
        await using var factory = new DireitoFacilApiFactory();
        var cidadao = await factory.CriarClienteAsync();

        // Act: o "balde" permite 5 uploads seguidos por usuário
        for (var i = 0; i < 5; i++)
            Assert.Equal(HttpStatusCode.Created, (await cidadao.Client.EnviarDocumentoAsync(TipoDocumentoEnum.Outro)).StatusCode);

        var sexto = await cidadao.Client.EnviarDocumentoAsync(TipoDocumentoEnum.Outro);

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, sexto.StatusCode);
    }

    [Fact]
    public async Task Upload_LimiteEPorUsuario_OutroCidadaoNaoEAfetado()
    {
        // Arrange
        await using var factory = new DireitoFacilApiFactory();
        var maria = await factory.CriarClienteAsync();
        var joao = await factory.CriarClienteAsync();

        for (var i = 0; i < 6; i++)
            await maria.Client.EnviarDocumentoAsync(TipoDocumentoEnum.Outro);

        // Act
        var resposta = await joao.Client.EnviarDocumentoAsync(TipoDocumentoEnum.Outro);

        // Assert
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }
}
