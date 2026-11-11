using System.ComponentModel.DataAnnotations;

namespace DireitoFacil.Application.DTOs;

/// <summary>
/// Monta o pacote de documentos de um pedido de direito.
/// </summary>
/// <param name="CasoId">Caso aberto pela API Java para este direito.</param>
/// <param name="DireitoCodigo">Código do direito no catálogo (ex.: CIPTEA, BPC_PCD).</param>
/// <param name="DocumentoIds">Documentos do cidadão que compõem o envio.</param>
public record CriarEnvioRequest(
    [Range(1, long.MaxValue, ErrorMessage = "O caso informado é inválido")]
    long CasoId,

    [Required(ErrorMessage = "O código do direito é obrigatório")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "O código do direito deve ter entre 2 e 60 caracteres")]
    string DireitoCodigo,

    [Required(ErrorMessage = "Informe os documentos do envio")]
    [MinLength(1, ErrorMessage = "O envio precisa ter ao menos um documento")]
    [MaxLength(20, ErrorMessage = "O envio pode ter no máximo 20 documentos")]
    IReadOnlyList<Guid> DocumentoIds
) : IRequestDto;
