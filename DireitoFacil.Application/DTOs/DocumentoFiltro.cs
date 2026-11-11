using DiretoFacil.Domain.Enums;

namespace DireitoFacil.Application.DTOs;

/// <summary>
/// Filtros e paginação da listagem de documentos (query string).
/// Para o cidadão, <see cref="CidadaoId"/> é sempre substituído pelo próprio id.
/// </summary>
/// <param name="Page">Página (1-based, padrão 1).</param>
/// <param name="PageSize">Itens por página (1 a 100, padrão 20).</param>
/// <param name="Tipo">Filtra pelo tipo.</param>
/// <param name="Status">Filtra pelo status.</param>
/// <param name="CidadaoId">Filtra pelo cidadão (só para o gestor).</param>
public record DocumentoFiltro(
    int Page = 1,
    int PageSize = 20,
    TipoDocumentoEnum? Tipo = null,
    StatusDocumentoEnum? Status = null,
    string? CidadaoId = null);
