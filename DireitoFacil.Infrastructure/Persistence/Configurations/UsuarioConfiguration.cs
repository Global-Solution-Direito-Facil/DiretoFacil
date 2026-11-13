using DireitoFacil.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DireitoFacil.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("DF_USUARIO");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nome).HasMaxLength(120).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(254).IsRequired();
        builder.Property(u => u.SenhaHash).HasMaxLength(200).IsRequired();
        builder.Property(u => u.Papel).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(u => u.CriadoEm).IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();
    }
}
