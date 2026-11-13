using System.Text;
using DireitoFacil.Application.DTOs;
using DiretoFacil.Domain.Entities;
using DiretoFacil.Domain.Enums;

namespace DireitoFacil.Application.Tests.Fakes;

/// <summary>
/// Dados de teste reutilizáveis.
/// </summary>
internal static class Dados
{
    public const string CidadaoA = "cidadao-a";
    public const string CidadaoB = "cidadao-b";
    public const string Gestor = "gestor-1";

    public static byte[] PdfBytes(string conteudo = "laudo") => Encoding.ASCII.GetBytes($"%PDF-1.7\n{conteudo}\n%%EOF");

    public static byte[] PngBytes() => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x01];

    public static ArquivoUpload Upload(byte[] bytes, string contentType = "application/pdf", string nome = "laudo.pdf")
        => new(nome, contentType, bytes.Length, new MemoryStream(bytes));

    public static Documento Documento(string cidadaoId = CidadaoA, string hash = "HASH")
        => new(cidadaoId, TipoDocumentoEnum.Rg, "rg.pdf", "application/pdf", 100, hash + Guid.NewGuid().ToString("N"), false);
}
