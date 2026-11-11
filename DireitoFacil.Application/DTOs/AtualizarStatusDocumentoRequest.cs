using System.ComponentModel.DataAnnotations;
using DiretoFacil.Domain.Enums;

namespace DireitoFacil.Application.DTOs;

/// <summary>
/// Resultado da extração informado pela API Java (papel SERVICO).
/// </summary>
/// <param name="Status">PROCESSANDO, EXTRAIDO ou FALHA.</param>
public record AtualizarStatusDocumentoRequest(
    [Required(ErrorMessage = "O status é obrigatório")]
    [EnumDataType(typeof(StatusDocumentoEnum), ErrorMessage = "Status inválido")]
    StatusDocumentoEnum Status
) : IRequestDto;
