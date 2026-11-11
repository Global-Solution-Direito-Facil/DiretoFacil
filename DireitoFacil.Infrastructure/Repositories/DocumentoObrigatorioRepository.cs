using DireitoFacil.Application.Repositories;
using DireitoFacil.Infrastructure.Persistence;
using DiretoFacil.Domain.Entites;
using DiretoFacil.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DireitoFacil.Infrastructure.Repositories;

public sealed class DocumentoObrigatorioRepository(DireitoFacilContext context) : IDocumentoObrigatorioRepository
{
    public Task<bool> DireitoExisteAsync(string direitoCodigo, CancellationToken cancellationToken = default)
    {
        var codigo = DocumentoObrigatorio.NormalizarCodigo(direitoCodigo);
        return context.DocumentosObrigatorios.AnyAsync(o => o.DireitoCodigo == codigo, cancellationToken);
    }

    public async Task<IReadOnlyList<TipoDocumentoEnum>> ObterTiposAsync(
        string direitoCodigo, CancellationToken cancellationToken = default)
    {
        var codigo = DocumentoObrigatorio.NormalizarCodigo(direitoCodigo);
        return await context.DocumentosObrigatorios
            .AsNoTracking()
            .Where(o => o.DireitoCodigo == codigo)
            .Select(o => o.TipoDocumento)
            .ToListAsync(cancellationToken);
    }
}
