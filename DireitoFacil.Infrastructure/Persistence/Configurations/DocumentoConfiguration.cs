using DiretoFacil.Domain.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DireitoFacil.Infrastructure.Persistence.Configurations;

public class DocumentoConfiguration : IEntityTypeConfiguration<Documento>
{
    public void Configure(EntityTypeBuilder<Documento> builder)
    {
        builder.ToTable("DF_DOCUMENTO");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.CidadaoId).HasMaxLength(254).IsRequired();
        builder.Property(d => d.Tipo).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(d => d.Descricao).HasMaxLength(300);
        builder.Property(d => d.NomeArquivo).HasMaxLength(255).IsRequired();
        builder.Property(d => d.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(d => d.TamanhoArquivo).IsRequired();
        builder.Property(d => d.BlobPath).HasMaxLength(400).IsRequired();
        builder.Property(d => d.HashSha256).HasMaxLength(64).IsRequired();
        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(d => d.ConsentimentoSaude).IsRequired();
        builder.Property(d => d.CriadoEm).IsRequired();

        // Segunda camada contra duplicata: o mesmo arquivo não entra duas vezes para o mesmo cidadão.
        builder.HasIndex(d => new { d.CidadaoId, d.HashSha256 }).IsUnique();
        builder.HasIndex(d => new { d.CidadaoId, d.CriadoEm });
    }
}
