using System.ComponentModel.DataAnnotations;
using DireitoFacil.Domain.Enums;

namespace DireitoFacil.Api.Controllers.Forms;

/// <summary>
/// Formulário multipart/form-data do upload de documento.
/// Existe só na Api porque <see cref="IFormFile"/> é um tipo do ASP.NET;
/// o controller converte para os DTOs da Application.
/// </summary>
public sealed class DocumentoUploadForm
{
    /// <summary>Arquivo PDF, JPEG ou PNG de até 10 MB.</summary>
    [Required(ErrorMessage = "O arquivo é obrigatório")]
    public IFormFile Arquivo { get; set; } = null!;

    /// <summary>Tipo do documento (ex.: Laudo, Rg, Cpf).</summary>
    [Required(ErrorMessage = "O tipo do documento é obrigatório")]
    [EnumDataType(typeof(TipoDocumentoEnum), ErrorMessage = "Tipo de documento inválido")]
    public TipoDocumentoEnum? Tipo { get; set; }

    /// <summary>Consentimento para dado de saúde. Obrigatório para Laudo (LGPD, art. 11).</summary>
    public bool ConsentimentoSaude { get; set; }

    /// <summary>Descrição opcional.</summary>
    [StringLength(300, ErrorMessage = "A descrição deve ter no máximo 300 caracteres")]
    public string? Descricao { get; set; }
}
