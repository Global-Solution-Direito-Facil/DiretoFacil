namespace DiretoFacil.Domain.Expections;

/// <summary>
/// Operação válida em si, mas proibida no estado atual da entidade (vira 409 Conflict).
/// Ex.: submeter um envio já aprovado.
/// </summary>
public sealed class RegraDeNegocioException(string message) : DomainException(message);
