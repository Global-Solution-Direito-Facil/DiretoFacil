using DireitoFacil.Application.DTOs;
using DireitoFacil.Application.Repositories;
using DireitoFacil.Application.Repositories.Interfaces;
using DireitoFacil.Infrastructure.Persistence;
using DireitoFacil.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DireitoFacil.Infrastructure.Repositories;

public sealed class EnvioRepository(DireitoFacilContext context) : Repository<Envio>(context), IEnvioRepository
{
    /// <summary>Sempre carrega os vínculos, porque a resposta inclui os ids dos documentos.</summary>
    public override Task<Envio?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => DbSet.Include(e => e.Documentos).FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<Envio?> ObterComDocumentosAsync(Guid id, CancellationToken cancellationToken = default)
        => DbSet
            .Include(e => e.Documentos)
            .ThenInclude(ed => ed.Documento)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<(IReadOnlyList<Envio> Items, int TotalItems)> GetPagedAsync(
        EnvioFiltro filtro, CancellationToken cancellationToken = default)
    {
        IQueryable<Envio> query = DbSet.AsNoTracking().Include(e => e.Documentos);

        if (!string.IsNullOrWhiteSpace(filtro.CidadaoId))
            query = query.Where(e => e.CidadaoId == filtro.CidadaoId);
        if (filtro.Status is not null)
            query = query.Where(e => e.Status == filtro.Status);

        return PaginarAsync(query, filtro.Page, filtro.PageSize, cancellationToken);
    }
}
