using DireitoFacil.Application.DTOs;
using DireitoFacil.Application.Repositories;
using DireitoFacil.Application.Repositories.Interfaces;
using DiretoFacil.Domain.Entities;
using DiretoFacil.Domain.Enums;

namespace DireitoFacil.Application.Tests.Fakes;

/// <summary>
/// "Banco" em memória compartilhado pelos repositórios falsos, para os testes de serviço
/// rodarem sem EF nem banco real.
/// </summary>
internal sealed class BancoEmMemoria : IUnitOfWork
{
    public List<Documento> Documentos { get; } = [];
    public List<Envio> Envios { get; } = [];
    public List<Usuario> Usuarios { get; } = [];
    public List<DocumentoObrigatorio> Obrigatorios { get; } = [];

    public int Salvamentos { get; private set; }
    public bool FalharAoSalvar { get; set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (FalharAoSalvar)
            throw new InvalidOperationException("Falha simulada no banco.");

        Salvamentos++;
        return Task.CompletedTask;
    }
}

internal sealed class FakeDocumentoRepository(BancoEmMemoria banco) : IDocumentoRepository
{
    public Task<Documento?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(banco.Documentos.FirstOrDefault(d => d.Id == id));

    public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(banco.Documentos.Any(d => d.Id == id));

    public void Add(Documento entity) => banco.Documentos.Add(entity);

    public void Remove(Documento entity) => banco.Documentos.Remove(entity);

    public Task<bool> ExisteHashAsync(string cidadaoId, string hashSha256, CancellationToken cancellationToken = default)
        => Task.FromResult(banco.Documentos.Any(d => d.CidadaoId == cidadaoId && d.HashSha256 == hashSha256));

    public Task<IReadOnlyList<Documento>> ObterDoCidadaoAsync(
        string cidadaoId, IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var set = ids.ToHashSet();
        IReadOnlyList<Documento> resultado = banco.Documentos
            .Where(d => d.CidadaoId == cidadaoId && set.Contains(d.Id))
            .ToList();
        return Task.FromResult(resultado);
    }

    public Task<bool> EstaEmEnvioAtivoAsync(Guid documentoId, CancellationToken cancellationToken = default)
        => Task.FromResult(banco.Envios.Any(e => e.EstaAtivo() && e.Documentos.Any(ed => ed.DocumentoId == documentoId)));

    public Task<(IReadOnlyList<Documento> Items, int TotalItems)> GetPagedAsync(
        DocumentoFiltro filtro, CancellationToken cancellationToken = default)
    {
        var query = banco.Documentos.AsEnumerable();
        if (filtro.CidadaoId is not null) query = query.Where(d => d.CidadaoId == filtro.CidadaoId);
        if (filtro.Tipo is not null) query = query.Where(d => d.Tipo == filtro.Tipo);
        if (filtro.Status is not null) query = query.Where(d => d.Status == filtro.Status);

        var lista = query.OrderBy(d => d.CriadoEm).ToList();
        IReadOnlyList<Documento> pagina = lista.Skip((filtro.Page - 1) * filtro.PageSize).Take(filtro.PageSize).ToList();
        return Task.FromResult((pagina, lista.Count));
    }
}

internal sealed class FakeEnvioRepository(BancoEmMemoria banco) : IEnvioRepository
{
    public Task<Envio?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(banco.Envios.FirstOrDefault(e => e.Id == id));

    public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(banco.Envios.Any(e => e.Id == id));

    public void Add(Envio entity) => banco.Envios.Add(entity);

    public void Remove(Envio entity) => banco.Envios.Remove(entity);

    public Task<Envio?> ObterComDocumentosAsync(Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public Task<(IReadOnlyList<Envio> Items, int TotalItems)> GetPagedAsync(
        EnvioFiltro filtro, CancellationToken cancellationToken = default)
    {
        var query = banco.Envios.AsEnumerable();
        if (filtro.CidadaoId is not null) query = query.Where(e => e.CidadaoId == filtro.CidadaoId);
        if (filtro.Status is not null) query = query.Where(e => e.Status == filtro.Status);

        var lista = query.OrderBy(e => e.CriadoEm).ToList();
        IReadOnlyList<Envio> pagina = lista.Skip((filtro.Page - 1) * filtro.PageSize).Take(filtro.PageSize).ToList();
        return Task.FromResult((pagina, lista.Count));
    }
}

internal sealed class FakeUsuarioRepository(BancoEmMemoria banco) : IUsuarioRepository
{
    public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(banco.Usuarios.FirstOrDefault(u => u.Id == id));

    public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(banco.Usuarios.Any(u => u.Id == id));

    public void Add(Usuario entity) => banco.Usuarios.Add(entity);

    public void Remove(Usuario entity) => banco.Usuarios.Remove(entity);

    public Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => Task.FromResult(banco.Usuarios.FirstOrDefault(u => u.Email == email));

    public Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default)
        => Task.FromResult(banco.Usuarios.Any(u => u.Email == email));
}

internal sealed class FakeDocumentoObrigatorioRepository(BancoEmMemoria banco) : IDocumentoObrigatorioRepository
{
    public Task<bool> DireitoExisteAsync(string direitoCodigo, CancellationToken cancellationToken = default)
        => Task.FromResult(banco.Obrigatorios.Any(o => o.DireitoCodigo == direitoCodigo));

    public Task<IReadOnlyList<TipoDocumentoEnum>> ObterTiposAsync(string direitoCodigo, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TipoDocumentoEnum> tipos = banco.Obrigatorios
            .Where(o => o.DireitoCodigo == direitoCodigo)
            .Select(o => o.TipoDocumento)
            .ToList();
        return Task.FromResult(tipos);
    }
}
