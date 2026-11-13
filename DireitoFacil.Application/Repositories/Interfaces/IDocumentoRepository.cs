using DireitoFacil.Application.DTOs;
using DiretoFacil.Domain.Entities;

namespace DireitoFacil.Application.Repositories.Interfaces;

public interface IDocumentoRepository : IRepository<Documento>
{
    /// <summary>Mesmo arquivo (hash) já enviado por este cidadão.</summary>
    Task<bool> ExisteHashAsync(string cidadaoId, string hashSha256, CancellationToken cancellationToken = default);

    /// <summary>Busca documentos pelos ids, retornando só os que pertencem ao cidadão.</summary>
    Task<IReadOnlyList<Documento>> ObterDoCidadaoAsync(
        string cidadaoId, IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>O documento faz parte de algum envio não cancelado.</summary>
    Task<bool> EstaEmEnvioAtivoAsync(Guid documentoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Página ordenada por CriadoEm, com Skip/Take e Count executados no banco.
    /// </summary>
    Task<(IReadOnlyList<Documento> Items, int TotalItems)> GetPagedAsync(
        DocumentoFiltro filtro, CancellationToken cancellationToken = default);
}
