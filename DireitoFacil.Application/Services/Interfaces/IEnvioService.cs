using DireitoFacil.Application.DTOs;

namespace DireitoFacil.Application.Services.Interfaces;

/// <summary>
/// Casos de uso de Envio.
/// </summary>
public interface IEnvioService
{
    Task<EnvioResponse> CriarAsync(CriarEnvioRequest request, CancellationToken cancellationToken = default);

    Task<PagedResponse<EnvioResponse>> ListarAsync(EnvioFiltro filtro, CancellationToken cancellationToken = default);

    Task<EnvioResponse> ObterAsync(Guid id, CancellationToken cancellationToken = default);

    Task<EnvioResponse> AtualizarAsync(Guid id, AtualizarEnvioRequest request, CancellationToken cancellationToken = default);

    Task<EnvioResponse> SubmeterAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Aprovação ou devolução pelo assistente social (papel GESTOR).</summary>
    Task<EnvioResponse> AnalisarAsync(Guid id, AnaliseEnvioRequest request, CancellationToken cancellationToken = default);

    Task CancelarAsync(Guid id, CancellationToken cancellationToken = default);
}
