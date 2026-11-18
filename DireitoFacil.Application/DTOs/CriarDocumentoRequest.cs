using System.ComponentModel.DataAnnotations;
using DireitoFacil.Domain.Enums;

namespace DireitoFacil.Application.DTOs;

/// <summary>
/// Campos de formulário enviados junto com o arquivo no upload (multipart/form-data).
/// </summary>
/// <param name="Tipo">Tipo do documento.</param>
/// <param name="ConsentimentoSaude">Consentimento específico para dado de saúde. Obrigatório para LAUDO (LGPD, art. 11).</param>
/// <param name="Descricao">Descrição opcional.</param>
/// <param name="PessoaId">Familiar a quem o documento pertence (id da pessoa na API Java). Opcional.</param>
public record CriarDocumentoRequest(
    [Required(ErrorMessage = "O tipo do documento é obrigatório")]
    [EnumDataType(typeof(TipoDocumentoEnum), ErrorMessage = "Tipo de documento inválido")]
    TipoDocumentoEnum Tipo,

    bool ConsentimentoSaude = false,

    [StringLength(300, ErrorMessage = "A descrição deve ter no máximo 300 caracteres")]
    string? Descricao = null,

    [Range(1, long.MaxValue, ErrorMessage = "O familiar informado é inválido")]
    long? PessoaId = null
) : IRequestDto;
