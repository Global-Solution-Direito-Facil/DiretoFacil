namespace DireitoFacil.Application.DTOs;

/// <summary>
/// Arquivo recebido no upload, já desacoplado do IFormFile do ASP.NET
/// (a Application não depende da camada web).
/// </summary>
/// <param name="NomeArquivo">Nome original do arquivo.</param>
/// <param name="ContentType">Tipo declarado pelo cliente (é conferido pela assinatura do arquivo).</param>
/// <param name="TamanhoBytes">Tamanho em bytes.</param>
/// <param name="Conteudo">Conteúdo do arquivo.</param>
public record ArquivoUpload(string NomeArquivo, string ContentType, long TamanhoBytes, Stream Conteudo);
