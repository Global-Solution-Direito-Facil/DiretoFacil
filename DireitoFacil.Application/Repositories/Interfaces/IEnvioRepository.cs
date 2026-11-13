using DireitoFacil.Application.DTOs;
using DiretoFacil.Domain.Entities;

namespace DireitoFacil.Application.Repositories.Interfaces;

public interface IEnvioRepository : IRepository<Envio>
{
    /// <summary>Carrega o envio com as associações e os documentos (necessário para Submeter).</summary>
    Task<Envio?> ObterComDocumentosAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Envio> Items, int TotalItems)> GetPagedAsync(
        EnvioFiltro filtro, CancellationToken cancellationToken = default);
}
