namespace DireitoFacil.Application.DTOs;

/// <summary>
/// Marca os DTOs de entrada. O filtro de idempotência da Api usa esta marca para saber
/// quais argumentos compõem a "impressão digital" da requisição.
/// </summary>
public interface IRequestDto;
