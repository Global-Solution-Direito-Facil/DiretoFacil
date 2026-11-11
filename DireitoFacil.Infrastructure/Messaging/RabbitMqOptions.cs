namespace DireitoFacil.Infrastructure.Messaging;

/// <summary>
/// Seção "RabbitMq" do appsettings. Sem Uri configurada, o outbox só acumula os eventos
/// (a API continua funcionando normalmente).
/// </summary>
public sealed class RabbitMqOptions
{
    public const string Secao = "RabbitMq";

    /// <summary>Ex.: amqps://usuario:senha@host/vhost (via variável de ambiente).</summary>
    public string? Uri { get; set; }

    /// <summary>Fila consumida pela API Java.</summary>
    public string FilaDocumentosRecebidos { get; set; } = "documentos.recebidos";

    /// <summary>Intervalo entre as varreduras do outbox.</summary>
    public int IntervaloSegundos { get; set; } = 5;

    /// <summary>Validade da URL de leitura do arquivo enviada no evento.</summary>
    public int ValidadeUrlMinutos { get; set; } = 60;

    public bool Configurado => !string.IsNullOrWhiteSpace(Uri);
}
