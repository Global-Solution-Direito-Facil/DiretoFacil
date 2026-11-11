using DireitoFacil.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DireitoFacil.Infrastructure.Persistence.Configurations;

public class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("DF_IDEMPOTENCIA");

        // A mesma chave pode ser usada por usuários diferentes ou em endpoints diferentes sem colidir.
        builder.HasKey(r => new { r.Chave, r.UsuarioId, r.Endpoint });

        builder.Property(r => r.UsuarioId).HasMaxLength(254).IsRequired();
        builder.Property(r => r.Endpoint).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Fingerprint).HasMaxLength(64).IsRequired();
        builder.Property(r => r.RespostaJson).HasColumnType("CLOB");
        builder.Property(r => r.CriadoEm).IsRequired();
        builder.Property(r => r.ExpiraEm).IsRequired();
        builder.Ignore(r => r.EstaConcluido);

        builder.HasIndex(r => r.ExpiraEm);
    }
}
