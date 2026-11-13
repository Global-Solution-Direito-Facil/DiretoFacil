using DireitoFacil.Application.DTOs;
using DireitoFacil.Application.Repositories;
using DireitoFacil.Application.Repositories.Interfaces;
using DireitoFacil.Infrastructure.Persistence;
using DiretoFacil.Domain.Entities;
using DiretoFacil.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DireitoFacil.Infrastructure.Repositories;

public sealed class DocumentoRepository(DireitoFacilContext context) : Repository<Documento>(context), IDocumentoRepository
{
    public Task<bool> ExisteHashAsync(string cidadaoId, string hashSha256, CancellationToken cancellationToken = default)
    {
        var hash = hashSha256.ToUpperInvariant();
        return DbSet.AnyAsync(d => d.CidadaoId == cidadaoId && d.HashSha256 == hash, cancellationToken);
    }

    public async Task<IReadOnlyList<Documento>> ObterDoCidadaoAsync(
        string cidadaoId, IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var lista = ids.Distinct().ToList();
        if (lista.Count == 0)
            return [];

        return await DbSet
            .Where(d => d.CidadaoId == cidadaoId && lista.Contains(d.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<bool> EstaEmEnvioAtivoAsync(Guid documentoId, CancellationToken cancellationToken = default)
        => Context.Envios.AnyAsync(
            e => e.Status != StatusEnvioEnum.Cancelado && e.Documentos.Any(ed => ed.DocumentoId == documentoId),
            cancellationToken);

    public Task<(IReadOnlyList<Documento> Items, int TotalItems)> GetPagedAsync(
        DocumentoFiltro filtro, CancellationToken cancellationToken = default)
    {
        IQueryable<Documento> query = DbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.CidadaoId))
            query = query.Where(d => d.CidadaoId == filtro.CidadaoId);
        if (filtro.Tipo is not null)
            query = query.Where(d => d.Tipo == filtro.Tipo);
        if (filtro.Status is not null)
            query = query.Where(d => d.Status == filtro.Status);

        return PaginarAsync(query, filtro.Page, filtro.PageSize, cancellationToken);
    }
}
