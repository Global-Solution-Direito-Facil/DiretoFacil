using DiretoFacil.Domain.Entites;
using DiretoFacil.Domain.Enums;

namespace DireitoFacil.Domain.Tests;

/// <summary>
/// Fábrica de objetos válidos para os testes, evitando repetir construtores longos.
/// </summary>
internal static class Fakes
{
    public const string CidadaoA = "cidadao-a";
    public const string CidadaoB = "cidadao-b";
    public const string Gestor = "gestor-1";

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
        envio.Submeter();
        return envio;
    }
}
