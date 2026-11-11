using System.Net;
using System.Net.Http.Json;
using DireitoFacil.Api.Tests.Infra;
using DireitoFacil.Application.DTOs;
using DiretoFacil.Domain.Enums;

namespace DireitoFacil.Api.Tests.Endpoints;

/// <summary>
/// Cadastro e login reais pela API. O rate limit de autenticação é 10/min por IP,
/// por isso esta classe faz poucas chamadas a /api/auth (cada classe tem a sua API).
/// </summary>
public class AuthEndpointsTests(DireitoFacilApiFactory factory) : IClassFixture<DireitoFacilApiFactory>
{
    private static RegistrarUsuarioRequest Cadastro(string email) => new("Maria Silva", email, "senha1234");

    [Fact]
    public async Task Registrar_DadosValidos_DeveRetornar201ComoCidadao()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var resposta = await client.PostAsJsonAsync("/api/auth/registrar", Cadastro("Maria.Registro@Teste.com"));

        // Assert
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var usuario = await resposta.LerAsync<UsuarioResponse>();
        Assert.Equal("maria.registro@teste.com", usuario.Email);
        Assert.Equal(PapelUsuarioEnum.Cidadao, usuario.Papel);
    }

    [Fact]
    public async Task Registrar_EmailJaCadastrado_DeveRetornar409()
    {
        // Arrange
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/registrar", Cadastro("duplicado@teste.com"));

        // Act
        var resposta = await client.PostAsJsonAsync("/api/auth/registrar", Cadastro("DUPLICADO@teste.com"));

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task Registrar_SenhaFraca_DeveRetornar400()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var resposta = await client.PostAsJsonAsync("/api/auth/registrar",
            new RegistrarUsuarioRequest("Maria", "fraca@teste.com", "123"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Login_CredenciaisValidas_DeveRetornarTokenQueDaAcessoAApi()
    {
        // Arrange
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/registrar", Cadastro("login@teste.com"));

        // Act
        var resposta = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("LOGIN@teste.com", "senha1234"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var token = await resposta.LerAsync<TokenResponse>();
        Assert.False(string.IsNullOrWhiteSpace(token.AccessToken));

        client.DefaultRequestHeaders.Authorization = new("Bearer", token.AccessToken);
        var protegido = await client.GetAsync("/api/documentos");
        Assert.Equal(HttpStatusCode.OK, protegido.StatusCode);
    }

    [Fact]
    public async Task Login_SenhaErrada_DeveRetornar401()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var resposta = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("ninguem@teste.com", "errada123"));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }
}
