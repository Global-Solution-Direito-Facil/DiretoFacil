namespace DiretoFacil.Domain.Enums;

/// <summary>
/// Ciclo do documento: RECEBIDO → PROCESSANDO (IA extraindo) → EXTRAIDO ou FALHA.
/// </summary>
public enum StatusDocumentoEnum
{
    Recebido,
    Processando,
    Extraido,
    Falha
}