namespace DireitoFacil.Application.DTOs;

/// <summary>
/// Conteúdo de um documento para download.
/// </summary>
public record ArquivoResponse(Stream Conteudo, string ContentType, string NomeArquivo);
