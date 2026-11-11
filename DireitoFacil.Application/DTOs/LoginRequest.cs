using System.ComponentModel.DataAnnotations;

namespace DireitoFacil.Application.DTOs;

/// <summary>
/// Credenciais de login.
/// </summary>
public record LoginRequest(
    [Required(ErrorMessage = "O e-mail é obrigatório")]
    string Email,

    [Required(ErrorMessage = "A senha é obrigatória")]
    string Senha
);
