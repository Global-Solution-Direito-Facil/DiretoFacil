using DireitoFacil.Application.Repositories;
using DireitoFacil.Application.Repositories.Interfaces;
using DireitoFacil.Infrastructure.Persistence;
using DiretoFacil.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace DireitoFacil.Infrastructure.Repositories;

/// <summary>
/// Implementação genérica do <see cref="IRepository{T}"/>. Add e Remove apenas marcam a
/// alteração no contexto; quem grava é o <see cref="IUnitOfWork"/> (o próprio contexto).
/// </summary>
public abstract class Repository<T>(DireitoFacilContext context) : IRepository<T> where T : BaseEntity
{
    protected readonly DireitoFacilContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public virtual Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => DbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => DbSet.AnyAsync(x => x.Id == id, cancellationToken);

    public void Add(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        DbSet.Add(entity);
    }

    public void Remove(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        DbSet.Remove(entity);
    }

    /// <summary>
    /// Count, OrderBy, Skip e Take são traduzidos para SQL: só os itens da página saem do banco.
    /// A ordenação por CriadoEm garante páginas estáveis.
    /// </summary>
    protected static async Task<(IReadOnlyList<T> Items, int TotalItems)> PaginarAsync(
        IQueryable<T> query, int page, int pageSize, CancellationToken cancellationToken)
    {
        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.CriadoEm)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalItems);
    }
}
