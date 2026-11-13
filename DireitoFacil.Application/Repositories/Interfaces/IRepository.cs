using DiretoFacil.Domain.Common;

namespace DireitoFacil.Application.Repositories.Interfaces;

/// <summary>
/// Contrato genérico de persistência para entidades que derivam de <see cref="BaseEntity"/>.
/// Add e Remove só marcam a alteração; a gravação acontece em <see cref="IUnitOfWork.SaveChangesAsync"/>,
/// o que permite salvar entidade + evento do outbox numa única transação.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Add(T entity);

    void Remove(T entity);
}
