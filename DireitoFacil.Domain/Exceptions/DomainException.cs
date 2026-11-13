namespace DireitoFacil.Domain.Exceptions;

/// <summary>
/// Dado inválido para o domínio (vira 400 no GlobalExceptionHandler).
/// </summary>
public  class DomainException(string message) : Exception(message);
