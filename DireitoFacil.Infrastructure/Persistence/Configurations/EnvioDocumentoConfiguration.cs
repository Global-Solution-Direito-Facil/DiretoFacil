using DiretoFacil.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DireitoFacil.Infrastructure.Persistence.Configurations;

public class EnvioDocumentoConfiguration : IEntityTypeConfiguration<EnvioDocumento>
{
    public void Configure(EntityTypeBuilder<EnvioDocumento> builder)
    {
        builder.ToTable("DF_ENVIO_DOCUMENTO");
        builder.HasKey(ed => new { ed.EnvioId, ed.DocumentoId });

        // O service já impede excluir documento de envio ativo; o cascade só limpa
        // vínculos de envios cancelados quando o documento é excluído.
        builder.HasOne(ed => ed.Documento)
            .WithMany()
            .HasForeignKey(ed => ed.DocumentoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
