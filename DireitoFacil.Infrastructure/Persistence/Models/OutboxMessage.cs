namespace DireitoFacil.Infrastructure.Persistence.Models;

/// <summary>
/// Evento aguardando publicação no RabbitMQ (padrão Outbox). É gravado na mesma transação
/// da entidade; o OutboxWorker publica depois e marca como processado.
/// </summary>
public class OutboxMessage
{
    public const int MaxTentativas = 10;

    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>Nome do evento, ex.: "DocumentoRecebidoEvent".</summary>
    public string Tipo { get; private set; } = null!;

    public string Payload { get; private set; } = null!;
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime? ProcessadoEm { get; private set; }
    public int Tentativas { get; private set; }
    public string? UltimoErro { get; private set; }

    protected OutboxMessage()
    {
    }

    public OutboxMessage(string tipo, string payload)
    {
        Tipo = tipo;
        Payload = payload;
    }

    public void MarcarProcessado()
    {
        ProcessadoEm = DateTime.UtcNow;
        UltimoErro = null;
    }

    public void RegistrarFalha(string erro)
    {
        Tentativas++;
        UltimoErro = erro.Length > 1000 ? erro[..1000] : erro;
    }
}
