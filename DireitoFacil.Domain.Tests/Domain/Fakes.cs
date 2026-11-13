using DireitoFacil.Domain.Entities;
using DireitoFacil.Domain.Enums;

namespace DireitoFacil.Domain.Tests.Domain;

/// <summary>
/// Fábrica de objetos válidos para os testes, evitando repetir construtores longos.
/// </summary>
internal static class Fakes
{
    public const string CidadaoA = "cidadao-a";
    public const string CidadaoB = "cidadao-b";
    public const string Gestor = "gestor-1";

    /// <summary>Nenhum documento obrigatório: para testes que não tratam dessa regra.</summary>
    public static readonly IReadOnlyCollection<TipoDocumentoEnum> SemObrigatorios = [];

    public static Documento Documento(
        string cidadaoId = CidadaoA,
        TipoDocumentoEnum tipo = TipoDocumentoEnum.Rg,
        bool consentimento = false,
        long tamanho = 1024,
        string contentType = "application/pdf")
        => new(cidadaoId, tipo, "arquivo.pdf", contentType, tamanho, "ABC123", consentimento);

    public static Envio Envio(params Documento[] documentos)
        => new(CidadaoA, 10, "ciptea", documentos.Length == 0 ? [Documento()] : documentos);

    public static Envio EnvioEnviado()
    {
        var envio = Envio();
        envio.Submeter(SemObrigatorios);
        return envio;
    }
}
