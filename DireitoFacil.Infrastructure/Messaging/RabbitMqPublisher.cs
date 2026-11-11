using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace DireitoFacil.Infrastructure.Messaging;

/// <summary>
/// Publica mensagens persistentes numa fila durável do RabbitMQ.
/// </summary>
public sealed class RabbitMqPublisher(IOptions<RabbitMqOptions> options) : IAsyncDisposable
{
    private readonly RabbitMqOptions _options = options.Value;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IConnection? _connection;

    public async Task PublicarAsync(string fila, string mensagemId, string payloadJson, CancellationToken cancellationToken)
    {
        var connection = await ObterConexaoAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: fila, durable: true, exclusive: false, autoDelete: false,
            arguments: null, cancellationToken: cancellationToken);

        var propriedades = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            MessageId = mensagemId
        };

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: fila,
            mandatory: false,
            basicProperties: propriedades,
            body: Encoding.UTF8.GetBytes(payloadJson),
            cancellationToken: cancellationToken);
    }

    /// <summary>Usado pelo health check.</summary>
    public async Task<bool> EstaConectavelAsync(CancellationToken cancellationToken)
    {
        var connection = await ObterConexaoAsync(cancellationToken);
        return connection.IsOpen;
    }

    private async Task<IConnection> ObterConexaoAsync(CancellationToken cancellationToken)
    {
        if (!_options.Configurado)
            throw new InvalidOperationException("RabbitMq:Uri não configurada.");

        if (_connection is { IsOpen: true })
            return _connection;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_connection is { IsOpen: true })
                return _connection;

            if (_connection is not null)
                await _connection.DisposeAsync();

            var factory = new ConnectionFactory
            {
                Uri = new Uri(_options.Uri!),
                RequestedConnectionTimeout = TimeSpan.FromSeconds(5)
            };

            _connection = await factory.CreateConnectionAsync("direitofacil-api", cancellationToken);
            return _connection;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();

        _lock.Dispose();
    }
}
