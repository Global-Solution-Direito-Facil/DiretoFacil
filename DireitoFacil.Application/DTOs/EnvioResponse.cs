using DiretoFacil.Domain.Entites;
using DiretoFacil.Domain.Enums;

namespace DireitoFacil.Application.DTOs;

/// <summary>
/// DTO de resposta para Envio.
/// </summary>
public record EnvioResponse(
    Guid Id,
    string CidadaoId,
    long CasoId,
    string DireitoCodigo,
    StatusEnvioEnum Status,
    string? Observacao,
    string? GestorId,
    DateTime? EnviadoEm,
    DateTime? AnalisadoEm,
    DateTime CriadoEm,
    IReadOnlyList<Guid> DocumentoIds)
{
    public static EnvioResponse FromDomain(Envio envio) => new(
        envio.Id,
        envio.CidadaoId,
        envio.CasoId,
        envio.DireitoCodigo,
        envio.Status,
        envio.Observacao,
        envio.GestorId,
        envio.EnviadoEm,
        envio.AnalisadoEm,
        envio.CriadoEm,
        envio.Documentos.Select(ed => ed.DocumentoId).ToList());
}
