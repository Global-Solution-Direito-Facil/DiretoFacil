using DireitoFacil.Infrastructure.Persistence.Seed;
using DiretoFacil.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DireitoFacil.Infrastructure.Persistence.Configurations;

public class DocumentoObrigatorioConfiguration : IEntityTypeConfiguration<DocumentoObrigatorio>
{
    public void Configure(EntityTypeBuilder<DocumentoObrigatorio> builder)
    {
        builder.ToTable("DF_DOCUMENTO_OBRIGATORIO");
        builder.HasKey(o => new { o.DireitoCodigo, o.TipoDocumento });

        builder.Property(o => o.DireitoCodigo).HasMaxLength(60).IsRequired();
        builder.Property(o => o.TipoDocumento).HasConversion<string>().HasMaxLength(40).IsRequired();

        // Carga inicial do catálogo (vira INSERT na migration e no script-bd.sql).
        builder.HasData(CatalogoDocumentosObrigatorios.Itens
            .Select(i => new { DireitoCodigo = i.Direito, TipoDocumento = i.Tipo })
            .Cast<object>());
    }
}
