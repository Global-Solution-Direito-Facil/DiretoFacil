using DireitoFacil.Infrastructure.Security;

namespace DireitoFacil.Infrastructure.Tests.Security;

public class SenhaHasherTests
{
    private readonly SenhaHasher _hasher = new();

    [Fact]
    public void GerarHash_NaoDeveGuardarSenhaEmTexto()
    {
        // Act
        var hash = _hasher.GerarHash("senha1234");

        // Assert
        Assert.DoesNotContain("senha1234", hash);
    }

    [Fact]
    public void GerarHash_MesmaSenhaDuasVezes_DeveGerarHashesDiferentes()
    {
        // Act: o salt é aleatório
        var primeiro = _hasher.GerarHash("senha1234");
        var segundo = _hasher.GerarHash("senha1234");

        // Assert
        Assert.NotEqual(primeiro, segundo);
        Assert.True(_hasher.Verificar(primeiro, "senha1234"));
        Assert.True(_hasher.Verificar(segundo, "senha1234"));
    }

    [Theory]
    [InlineData("senhaErrada1")]
    [InlineData("")]
    public void Verificar_SenhaIncorreta_DeveRetornarFalse(string senha)
    {
        // Arrange
        var hash = _hasher.GerarHash("senha1234");

        // Act + Assert
        Assert.False(_hasher.Verificar(hash, senha));
    }

    [Fact]
    public void Verificar_HashCorrompido_DeveRetornarFalseSemLancar()
    {
        // Act + Assert
        Assert.False(_hasher.Verificar("isto-nao-e-um-hash", "senha1234"));
    }
}
