using DiretoFacil.Domain.Entites;
using DiretoFacil.Domain.Enums;
using DiretoFacil.Domain.Exceptions;

namespace DireitoFacil.Domain.Tests;

public class UsuarioTests
{
    [Fact]
    public void Construtor_ComDadosValidos_DeveCriarCidadaoComEmailNormalizado()
    {
        // Act
        var usuario = new Usuario("  Maria Silva ", "  Maria.Silva@Email.COM ", "hash");

        // Assert
        Assert.Equal("Maria Silva", usuario.Nome);
        Assert.Equal("maria.silva@email.com", usuario.Email);
        Assert.Equal(PapelUsuarioEnum.Cidadao, usuario.Papel);
    }

    [Theory]
    [InlineData("")]
    [InlineData("sem-arroba")]
    [InlineData("a@")]
    [InlineData("Nome <a@b.com>")]
    public void Construtor_ComEmailInvalido_DeveLancarDomainException(string email)
    {
        // Act
        var act = () => new Usuario("Maria", email, "hash");

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    public void Construtor_ComNomeInvalido_DeveLancarDomainException(string nome)
    {
        // Act
        var act = () => new Usuario(nome, "a@b.com", "hash");

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Construtor_SemHashDeSenha_DeveLancarDomainException()
    {
        // Act
        var act = () => new Usuario("Maria", "a@b.com", " ");

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Construtor_ComPapelInexistente_DeveLancarDomainException()
    {
        // Act
        var act = () => new Usuario("Maria", "a@b.com", "hash", (PapelUsuarioEnum)99);

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void AlterarSenha_DeveTrocarHashERegistrarAtualizacao()
    {
        // Arrange
        var usuario = new Usuario("Maria", "a@b.com", "hash-antigo");

        // Act
        usuario.AlterarSenha("hash-novo");

        // Assert
        Assert.Equal("hash-novo", usuario.SenhaHash);
        Assert.NotNull(usuario.AtualizadoEm);
    }
}
