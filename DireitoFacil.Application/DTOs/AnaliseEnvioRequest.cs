using System.ComponentModel.DataAnnotations;
using DireitoFacil.Domain.Enums;

namespace DireitoFacil.Application.DTOs;

/// <summary>
/// Decisão do assistente social sobre um envio.
/// </summary>
/// <param name="Decisao">APROVAR ou DEVOLVER.</param>
/// <param name="Observacao">Obrigatória ao devolver: o que a família precisa corrigir.</param>
public record AnaliseEnvioRequest(
    [Required(ErrorMessage = "A decisão é obrigatória")]
    [EnumDataType(typeof(DecisaoAnaliseEnum), ErrorMessage = "Decisão inválida")]
    DecisaoAnaliseEnum Decisao,

    [StringLength(1000, ErrorMessage = "A observação deve ter no máximo 1000 caracteres")]
    string? Observacao = null
) : IRequestDto;
