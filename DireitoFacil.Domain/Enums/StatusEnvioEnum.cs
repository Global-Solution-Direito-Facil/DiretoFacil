namespace DireitoFacil.Domain.Enums;

/// <summary>
/// RASCUNHO → ENVIADO → APROVADO | DEVOLVIDO → (corrige) → ENVIADO.
/// RASCUNHO e DEVOLVIDO podem ser CANCELADOS.
/// </summary>
public enum StatusEnvioEnum
{
    Rascunho,
    Enviado,
    Aprovado,
    Devolvido,
    Cancelado
}