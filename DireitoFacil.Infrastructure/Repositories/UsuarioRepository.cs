using DireitoFacil.Application.Repositories;
using DireitoFacil.Infrastructure.Persistence;
using DiretoFacil.Domain.Entites;
using Microsoft.EntityFrameworkCore;

namespace DireitoFacil.Infrastructure.Repositories;

public sealed class UsuarioRepository(DireitoFacilContext context) : Repository<Usuario>(context), IUsuarioRepository
{
    public Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => DbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default)
        => DbSet.AnyAsync(u => u.Email == email, cancellationToken);
}
