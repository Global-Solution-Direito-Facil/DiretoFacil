using System.Text.Json;
using DireitoFacil.Application.Abstractions;
using DireitoFacil.Infrastructure.Persistence;
using DireitoFacil.Infrastructure.Persistence.Models;

namespace DireitoFacil.Infrastructure.Messaging;

/// <summary>
/// Não fala com o RabbitMQ: só adiciona o evento à tabela DF_OUTBOX no mesmo contexto.
/// Ele é gravado junto com a entidade no SaveChanges e publicado depois pelo <see cref="OutboxWorker"/>.
/// </summary>
public sealed class OutboxEventPublisher(DireitoFacilContext context) : IEventPublisher
{
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Publicar<TEvento>(TEvento evento) where TEvento : class
    {
        ArgumentNullException.ThrowIfNull(evento);

        var payload = JsonSerializer.Serialize(evento, JsonOptions);
        context.OutboxMessages.Add(new OutboxMessage(typeof(TEvento).Name, payload));
    }
}
