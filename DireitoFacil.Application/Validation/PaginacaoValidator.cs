namespace DireitoFacil.Application.Validation;

/// <summary>
/// Regras de paginação compartilhadas pelas listagens.
/// </summary>
public static class PaginacaoValidator
{
    public const int MinPageSize = 1;
    public const int MaxPageSize = 100;

    /// <exception cref="ArgumentException">Página ou tamanho fora do intervalo permitido (vira 400).</exception>
    public static void Validar(int page, int pageSize)
    {
        if (page < 1)
            throw new ArgumentException("O parâmetro 'page' deve ser maior ou igual a 1.", nameof(page));

        if (pageSize < MinPageSize || pageSize > MaxPageSize)
            throw new ArgumentException(
                $"O parâmetro 'pageSize' deve estar entre {MinPageSize} e {MaxPageSize}.", nameof(pageSize));
    }
}
