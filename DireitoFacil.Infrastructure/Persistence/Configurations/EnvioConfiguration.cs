using DiretoFacil.Domain.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DireitoFacil.Infrastructure.Persistence.Configurations;

public class EnvioConfiguration : IEntityTypeConfiguration<Envio>
{
    public void Configure(EntityTypeBuilder<Envio> builder)
    {
        builder.ToTable("DF_ENVIO");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CidadaoId).HasMaxLength(254).IsRequired();
        builder.Property(e => e.CasoId).IsRequired();
        builder.Property(e => e.DireitoCodigo).HasMaxLength(60).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.Observacao).HasMaxLength(1000);
        builder.Property(e => e.GestorId).HasMaxLength(254);
        builder.Property(e => e.CriadoEm).IsRequired();

        // A coleção é exposta como IReadOnlyCollection; o EF grava pelo campo privado _documentos.
        builder.HasMany(e => e.Documentos)
            .WithOne()
            .HasForeignKey(ed => ed.EnvioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(e => e.Documentos)
            .HasField("_documentos")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(e => new { e.CidadaoId, e.CriadoEm });
        builder.HasIndex(e => e.Status);
    }
}
