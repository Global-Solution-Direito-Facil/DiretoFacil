namespace DireitoFacil.Application.DTOs;

/// <summary>
/// Envelope de paginação genérico retornado pelas listagens paginadas.
/// </summary>
/// <param name="Page">Página atual (1-based).</param>
/// <param name="PageSize">Quantidade de itens por página.</param>
/// <param name="TotalItems">Total de itens existentes, sem considerar a paginação.</param>
/// <param name="TotalPages">Total de páginas: ceiling(TotalItems / PageSize).</param>
/// <param name="Items">Itens pertencentes à página atual.</param>
public record PagedResponse<T>(
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages,
    IReadOnlyList<T> Items)
{
    /// <summary>Indica se existe uma página anterior.</summary>
    public bool HasPrevious => Page > 1;

    /// <summary>Indica se existe uma próxima página.</summary>
    public bool HasNext => Page < TotalPages;

    public static PagedResponse<T> Create(IReadOnlyList<T> items, int page, int pageSize, int totalItems)
    {
        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PagedResponse<T>(page, pageSize, totalItems, totalPages, items);
    }
}
