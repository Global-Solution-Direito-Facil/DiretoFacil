using DireitoFacil.Domain.Enums;

namespace DireitoFacil.Api.Security;

/// <summary>
/// Valores da claim "role" e nomes das policies de autorização.
/// Os valores precisam ser idênticos aos usados pela API Java.
/// </summary>
public static class Papeis
{
    public const string Cidadao = "CIDADAO";
    public const string Gestor = "GESTOR";
    public const string Servico = "SERVICO";

    public static string DoEnum(PapelUsuarioEnum papel) => papel switch
    {
        PapelUsuarioEnum.Cidadao => Cidadao,
        PapelUsuarioEnum.Gestor => Gestor,
        PapelUsuarioEnum.Servico => Servico,
        _ => throw new ArgumentOutOfRangeException(nameof(papel))
    };
}

/// <summary>
/// Nomes das policies usadas em [Authorize(Policy = ...)].
/// </summary>
public static class Policies
{
    public const string SomenteCidadao = "SomenteCidadao";
    public const string SomenteGestor = "SomenteGestor";
    public const string SomenteServico = "SomenteServico";
}
