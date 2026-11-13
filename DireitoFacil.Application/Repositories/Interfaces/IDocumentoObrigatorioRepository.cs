using DiretoFacil.Domain.Enums;

namespace DireitoFacil.Application.Repositories.Interfaces;

/// <summary>
/// Leitura da tabela de documentos obrigatórios por direito (carregada por seed).
/// </summary>
public interface IDocumentoObrigatorioRepository
{
    /// <summary>O código do direito está cadastrado na tabela.</summary>
    Task<bool> DireitoExisteAsync(string direitoCodigo, CancellationToken cancellationToken = default);

    /// <summary>Tipos de documento exigidos pelo direito.</summary>
    Task<IReadOnlyList<TipoDocumentoEnum>> ObterTiposAsync(string direitoCodigo, CancellationToken cancellationToken = default);
}
