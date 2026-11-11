using DireitoFacil.Infrastructure.Persistence.Models;

namespace DireitoFacil.Infrastructure.Tests.Persistence;

public class ModelsTests
{
    [Fact]
    public void IdempotencyRecord_Novo_DeveEstarEmProcessamentoEExpirarEm24h()
    {
        // Act
        var registro = new IdempotencyRecord(Guid.NewGuid(), "usuario", "POST /api/envios", "FP", TimeSpan.FromHours(24));

        // Assert
        Assert.False(registro.EstaConcluido);
        Assert.False(registro.EstaExpirado(DateTime.UtcNow));
        Assert.True(registro.EstaExpirado(DateTime.UtcNow.AddHours(25)));
    }

    [Fact]
    public void IdempotencyRecord_Concluir_DeveGuardarStatusEResposta()
    {
        // Arrange
        var registro = new IdempotencyRecord(Guid.NewGuid(), "usuario", "POST /api/envios", "FP", TimeSpan.FromHours(1));

        // Act
        registro.Concluir(201, "{\"id\":1}");

        // Assert
        Assert.True(registro.EstaConcluido);
        Assert.Equal(201, registro.StatusCode);
        Assert.Equal("{\"id\":1}", registro.RespostaJson);
    }

    [Fact]
    public void OutboxMessage_RegistrarFalhaEDepoisProcessar_DeveContarTentativasELimparErro()
    {
        // Arrange
        var mensagem = new OutboxMessage("DocumentoRecebidoEvent", "{}");

        // Act
        mensagem.RegistrarFalha("RabbitMQ fora do ar");
        mensagem.RegistrarFalha(new string('x', 2000));
        mensagem.MarcarProcessado();

        // Assert
        Assert.Equal(2, mensagem.Tentativas);
        Assert.NotNull(mensagem.ProcessadoEm);
        Assert.Null(mensagem.UltimoErro);
    }

    [Fact]
    public void OutboxMessage_ErroMuitoLongo_DeveSerTruncadoPara1000Caracteres()
    {
        // Arrange
        var mensagem = new OutboxMessage("Evento", "{}");

        // Act
        mensagem.RegistrarFalha(new string('x', 2000));

        // Assert
        Assert.Equal(1000, mensagem.UltimoErro!.Length);
    }
}
