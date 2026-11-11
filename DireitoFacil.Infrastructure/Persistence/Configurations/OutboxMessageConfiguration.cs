using DireitoFacil.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DireitoFacil.Infrastructure.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("DF_OUTBOX");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Tipo).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Payload).HasColumnType("CLOB").IsRequired();
        builder.Property(m => m.CriadoEm).IsRequired();
        builder.Property(m => m.UltimoErro).HasMaxLength(1000);

        builder.HasIndex(m => new { m.ProcessadoEm, m.CriadoEm });
    }
}
