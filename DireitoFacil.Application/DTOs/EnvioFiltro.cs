using DiretoFacil.Domain.Enums;

namespace DireitoFacil.Application.DTOs;

/// <summary>
/// Filtros e paginação da listagem de envios. O gestor usa Status=Enviado como fila de análise.
/// </summary>
public record EnvioFiltro(
    int Page = 1,
    int PageSize = 20,
    StatusEnvioEnum? Status = null,
    string? CidadaoId = null);
