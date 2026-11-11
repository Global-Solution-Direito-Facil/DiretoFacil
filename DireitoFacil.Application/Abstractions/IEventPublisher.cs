namespace DireitoFacil.Application.Abstractions;

/// <summary>
/// Registra um evento para publicação (padrão Outbox): o evento é gravado na mesma transação
/// da entidade e um worker o publica no RabbitMQ depois. Nada é salvo até o
/// <see cref="Repositories.IUnitOfWork.SaveChangesAsync"/>.
/// </summary>
public interface IEventPublisher
{
    void Publicar<TEvento>(TEvento evento) where TEvento : class;
}
