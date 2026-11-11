using DiretoFacil.Domain.Exceptions;

namespace DireitoFacil.Application.Validation;

/// <summary>
/// Confere a assinatura binária (magic bytes) do arquivo. O Content-Type vem do cliente e
/// pode mentir: um executável renomeado para .pdf é barrado aqui.
/// </summary>
public static class ArquivoValidator
{
    private static readonly Dictionary<string, byte[][]> Assinaturas = new(StringComparer.OrdinalIgnoreCase)
    {
        ["application/pdf"] = [[0x25, 0x50, 0x44, 0x46]],                          // %PDF
        ["image/png"] = [[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]],         // .PNG....
        ["image/jpeg"] = [[0xFF, 0xD8, 0xFF]]                                        // JPEG SOI
    };

    /// <summary>
    /// Lança <see cref="DomainException"/> se o conteúdo não corresponder ao tipo declarado.
    /// O stream precisa permitir Seek; a posição é restaurada ao final.
    /// </summary>
    public static void ValidarAssinatura(Stream conteudo, string contentType)
    {
        ArgumentNullException.ThrowIfNull(conteudo);

        if (!conteudo.CanSeek)
            throw new ArgumentException("O stream do arquivo precisa permitir Seek.", nameof(conteudo));

        if (string.IsNullOrWhiteSpace(contentType) || !Assinaturas.TryGetValue(contentType, out var assinaturas))
            throw new DomainException("Formato não permitido. Envie PDF, JPEG ou PNG.");

        var posicaoOriginal = conteudo.Position;
        var cabecalho = new byte[assinaturas.Max(a => a.Length)];
        conteudo.Position = 0;
        var lidos = conteudo.ReadAtLeast(cabecalho, cabecalho.Length, throwOnEndOfStream: false);
        conteudo.Position = posicaoOriginal;

        var confere = assinaturas.Any(a => lidos >= a.Length && cabecalho.AsSpan(0, a.Length).SequenceEqual(a));

        if (!confere)
            throw new DomainException("O conteúdo do arquivo não corresponde ao formato informado.");
    }
}
