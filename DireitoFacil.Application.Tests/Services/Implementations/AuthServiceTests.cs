using DireitoFacil.Application.DTOs;
using DireitoFacil.Application.Exceptions;
using DireitoFacil.Application.Services.Implementations;
using DireitoFacil.Application.Tests.Fakes;
using DiretoFacil.Domain.Entites;
using DiretoFacil.Domain.Enums;
using DiretoFacil.Domain.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;

namespace DireitoFacil.Application.Tests.Services.Implementations;

public class AuthServiceTests
{
    private readonly BancoEmMemoria _banco = new();
    private readonly FakeSenhaHasher _hasher = new();

    private AuthService CriarService() => new(
        new FakeUsuarioRepository(_banco),
        _hasher,
        new FakeTokenService(),
        _banco,
        NullLogger<AuthService>.Instance);

    private static RegistrarUsuarioRequest Cadastro(string email = "maria@email.com")
        => new("Maria Silva", email, "senha1234");

    [Fact]
    public async Task RegistrarAsync_DadosValidos_DeveCriarCidadaoComSenhaEmHash()
    {
        // Arrange
        var service = CriarService();

        // Act
        var response = await service.RegistrarAsync(Cadastro("  Maria@Email.com "));

        // Assert
        var usuario = Assert.Single(_banco.Usuarios);
        Assert.Equal("maria@email.com", usuario.Email);
        Assert.Equal(PapelUsuarioEnum.Cidadao, usuario.Papel);
        Assert.NotEqual("senha1234", usuario.SenhaHash);
        Assert.Equal(usuario.Id, response.Id);
        Assert.Equal(1, _banco.Salvamentos);
    }

    [Fact]
    public async Task RegistrarAsync_EmailJaCadastradoComOutraCaixa_DeveLancarRegraDeNegocioException()
    {
        // Arrange
        var service = CriarService();
        await service.RegistrarAsync(Cadastro("maria@email.com"));

        // Act
        var act = () => service.RegistrarAsync(Cadastro("MARIA@EMAIL.COM"));

        // Assert
        await Assert.ThrowsAsync<RegraDeNegocioException>(act);
        Assert.Single(_banco.Usuarios);
    }

    [Fact]
    public async Task LoginAsync_CredenciaisCorretas_DeveRetornarToken()
    {
        // Arrange
        var service = CriarService();
        await service.RegistrarAsync(Cadastro());

        // Act
        var token = await service.LoginAsync(new LoginRequest("MARIA@email.com", "senha1234"));

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token.AccessToken));
        Assert.Equal("Bearer", token.TokenType);
        Assert.Equal("maria@email.com", token.Usuario.Email);
    }

    [Fact]
    public async Task LoginAsync_SenhaErrada_DeveLancarCredenciaisInvalidasException()
    {
        // Arrange
        var service = CriarService();
        await service.RegistrarAsync(Cadastro());

        // Act
        var act = () => service.LoginAsync(new LoginRequest("maria@email.com", "errada123"));

        // Assert
        await Assert.ThrowsAsync<CredenciaisInvalidasException>(act);
    }

    [Fact]
    public async Task LoginAsync_EmailInexistente_DeveLancarAMesmaExcecaoDaSenhaErrada()
    {
        // Arrange
        var service = CriarService();

        // Act
        var act = () => service.LoginAsync(new LoginRequest("ninguem@email.com", "senha1234"));

        // Assert
        var ex = await Assert.ThrowsAsync<CredenciaisInvalidasException>(act);
        Assert.Equal("E-mail ou senha inválidos.", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_Gestor_DeveRetornarTokenComPapelGestor()
    {
        // Arrange: gestor criado por seed, não pelo cadastro.
        _banco.Usuarios.Add(new Usuario("Assistente Social", "gestor@cras.gov.br", _hasher.GerarHash("gestor123"), PapelUsuarioEnum.Gestor));
        var service = CriarService();

        // Act
        var token = await service.LoginAsync(new LoginRequest("gestor@cras.gov.br", "gestor123"));

        // Assert
        Assert.Equal(PapelUsuarioEnum.Gestor, token.Usuario.Papel);
    }
}
