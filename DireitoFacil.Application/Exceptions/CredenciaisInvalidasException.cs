namespace DireitoFacil.Application.Exceptions;

/// <summary>
/// Login com e-mail ou senha incorretos (vira 401). A mensagem é sempre a mesma nos dois casos
/// para não revelar quais e-mails estão cadastrados.
/// </summary>
public sealed class CredenciaisInvalidasException() : Exception("E-mail ou senha inválidos.");
