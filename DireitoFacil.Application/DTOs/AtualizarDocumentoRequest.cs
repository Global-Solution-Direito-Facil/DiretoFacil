using System.ComponentModel.DataAnnotations;
using DiretoFacil.Domain.Enums;

namespace DireitoFacil.Application.DTOs;

/// <summary>
/// Reclassificação de um documento pela família.
/// </summary>
/// <param name="Tipo">Novo tipo do documento.</param>
/// <param name="Descricao">Nova descrição.</param>
/// <param name="ConsentimentoSaude">Necessário ao reclassificar para LAUDO.</param>
public record AtualizarDocumentoRequest(
    [Required(ErrorMessage = "O tipo do documento é obrigatório")]
    [EnumDataType(typeof(TipoDocumentoEnum), ErrorMessage = "Tipo de documento inválido")]
    TipoDocumentoEnum Tipo,

    [StringLength(300, ErrorMessage = "A descrição deve ter no máximo 300 caracteres")]
    string? Descricao = null,

    bool ConsentimentoSaude = false
) : IRequestDto;
