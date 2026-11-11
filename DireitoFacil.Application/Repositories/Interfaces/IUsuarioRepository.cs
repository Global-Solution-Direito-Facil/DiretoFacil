using DiretoFacil.Domain.Entites;

namespace DireitoFacil.Application.Repositories;

public interface IUsuarioRepository : IRepository<Usuario>
{
    /// <summary>Busca pelo e-mail já normalizado (minúsculas).</summary>
    Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default);
}
