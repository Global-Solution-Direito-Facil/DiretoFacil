namespace DireitoFacil.Application.Repositories;

/// <summary>
/// Confirma numa única transação tudo o que os repositórios marcaram.
/// </summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
