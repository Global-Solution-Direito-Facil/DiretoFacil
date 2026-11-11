using DireitoFacil.Application.Repositories;
using DireitoFacil.Infrastructure.Persistence.Models;
using DiretoFacil.Domain.Entites;
using Microsoft.EntityFrameworkCore;

namespace DireitoFacil.Infrastructure.Persistence;

/// <summary>
/// Contexto do EF Core (Oracle). Também é o <see cref="IUnitOfWork"/>: um único SaveChanges
/// grava entidades e mensagens do outbox na mesma transação.
/// </summary>
public class DireitoFacilContext(DbContextOptions<DireitoFacilContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Documento> Documentos => Set<Documento>();
    public DbSet<Envio> Envios => Set<Envio>();
    public DbSet<EnvioDocumento> EnvioDocumentos => Set<EnvioDocumento>();
    public DbSet<DocumentoObrigatorio> DocumentosObrigatorios => Set<DocumentoObrigatorio>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
        => await SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(DireitoFacilContext).Assembly);
}
