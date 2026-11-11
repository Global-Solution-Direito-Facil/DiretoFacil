using System.ComponentModel.DataAnnotations;

namespace DireitoFacil.Application.DTOs;

/// <summary>
/// Cadastro de cidadão pelo app. Gestores não se cadastram por aqui (são criados por seed).
/// </summary>
/// <param name="Nome">Nome completo.</param>
/// <param name="Email">E-mail usado no login.</param>
/// <param name="Senha">Senha com 8 a 100 caracteres, contendo letra e número.</param>
public record RegistrarUsuarioRequest(
    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 120 caracteres")]
    string Nome,

    [Required(ErrorMessage = "O e-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    [StringLength(254, ErrorMessage = "O e-mail deve ter no máximo 254 caracteres")]
    string Email,

    [Required(ErrorMessage = "A senha é obrigatória")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 100 caracteres")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "A senha deve conter letras e números")]
    string Senha
) : IRequestDto;
