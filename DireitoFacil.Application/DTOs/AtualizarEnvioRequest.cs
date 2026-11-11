using System.ComponentModel.DataAnnotations;

namespace DireitoFacil.Application.DTOs;

/// <summary>
/// Troca os documentos de um envio em RASCUNHO ou DEVOLVIDO.
/// </summary>
public record AtualizarEnvioRequest(
    [Required(ErrorMessage = "Informe os documentos do envio")]
    [MinLength(1, ErrorMessage = "O envio precisa ter ao menos um documento")]
    [MaxLength(20, ErrorMessage = "O envio pode ter no máximo 20 documentos")]
    IReadOnlyList<Guid> DocumentoIds
) : IRequestDto;
