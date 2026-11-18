using DireitoFacil.Domain.Entities;
using DireitoFacil.Domain.Enums;

namespace DireitoFacil.Application.DTOs;

/// <summary>
/// DTO de resposta para Documento. Não expõe o caminho interno do arquivo nem o hash.
/// </summary>
public record DocumentoResponse(
    Guid Id,
    string CidadaoId,
    TipoDocumentoEnum Tipo,
    string? Descricao,
    string NomeArquivo,
    string ContentType,
    long TamanhoArquivo,
    StatusDocumentoEnum Status,
    bool ConsentimentoSaude,
    DateTime? ConsentimentoSaudeEm,
    DateTime CriadoEm,
    DateTime? AtualizadoEm,
    long? PessoaId = null)
{
    public static DocumentoResponse FromDomain(Documento documento) => new(
        documento.Id,
        documento.CidadaoId,
        documento.Tipo,
        documento.Descricao,
        documento.NomeArquivo,
        documento.ContentType,
        documento.TamanhoArquivo,
        documento.Status,
        documento.ConsentimentoSaude,
        documento.ConsentimentoSaudeEm,
        documento.CriadoEm,
        documento.AtualizadoEm,
        documento.PessoaId);
}
