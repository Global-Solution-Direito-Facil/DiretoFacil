using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DireitoFacil.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AtualizaCatalogoDireitos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "FARMACIA_POPULAR", "Cpf" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "FARMACIA_POPULAR", "Receita" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "FARMACIA_POPULAR", "Rg" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "TARIFA_SOCIAL", "ComprovanteResidencia" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "TARIFA_SOCIAL", "Cpf" });

            migrationBuilder.InsertData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                columns: new[] { "DireitoCodigo", "TipoDocumento" },
                values: new object[,]
                {
                    { "BOLSA_FAMILIA", "ComprovanteRenda" },
                    { "BOLSA_FAMILIA", "ComprovanteResidencia" },
                    { "BOLSA_FAMILIA", "Cpf" },
                    { "BOLSA_FAMILIA", "Rg" },
                    { "COTA_PCD_CONCURSO", "Cpf" },
                    { "COTA_PCD_CONCURSO", "Laudo" },
                    { "COTA_PCD_CONCURSO", "Rg" },
                    { "COTA_PCD_EMPREGO", "Cpf" },
                    { "COTA_PCD_EMPREGO", "Laudo" },
                    { "COTA_PCD_EMPREGO", "Rg" },
                    { "COTA_PCD_ENSINO_SUPERIOR", "Cpf" },
                    { "COTA_PCD_ENSINO_SUPERIOR", "Laudo" },
                    { "COTA_PCD_ENSINO_SUPERIOR", "Rg" },
                    { "CREDENCIAL_IDOSO", "ComprovanteResidencia" },
                    { "CREDENCIAL_IDOSO", "Rg" },
                    { "CREDENCIAL_PCD", "ComprovanteResidencia" },
                    { "CREDENCIAL_PCD", "Laudo" },
                    { "CREDENCIAL_PCD", "Rg" },
                    { "ISENCAO_IOF_VEICULO_PCD", "Cpf" },
                    { "ISENCAO_IOF_VEICULO_PCD", "Laudo" },
                    { "ISENCAO_IOF_VEICULO_PCD", "Rg" },
                    { "ISENCAO_IPI_VEICULO_PCD", "Cpf" },
                    { "ISENCAO_IPI_VEICULO_PCD", "Laudo" },
                    { "ISENCAO_IPI_VEICULO_PCD", "Rg" },
                    { "ISENCAO_IR_65MAIS", "Cpf" },
                    { "ISENCAO_IR_65MAIS", "Rg" },
                    { "MEIA_ENTRADA_PCD", "Laudo" },
                    { "MEIA_ENTRADA_PCD", "Rg" },
                    { "PASSE_IDOSO_INTERESTADUAL", "ComprovanteRenda" },
                    { "PASSE_IDOSO_INTERESTADUAL", "Rg" },
                    { "PRIORIDADE_IDOSO", "Rg" },
                    { "PRIORIDADE_PCD", "Laudo" },
                    { "PRIORIDADE_PCD", "Rg" },
                    { "TARIFA_SOCIAL_ENERGIA", "ComprovanteResidencia" },
                    { "TARIFA_SOCIAL_ENERGIA", "Cpf" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "BOLSA_FAMILIA", "ComprovanteRenda" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "BOLSA_FAMILIA", "ComprovanteResidencia" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "BOLSA_FAMILIA", "Cpf" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "BOLSA_FAMILIA", "Rg" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "COTA_PCD_CONCURSO", "Cpf" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "COTA_PCD_CONCURSO", "Laudo" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "COTA_PCD_CONCURSO", "Rg" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "COTA_PCD_EMPREGO", "Cpf" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "COTA_PCD_EMPREGO", "Laudo" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "COTA_PCD_EMPREGO", "Rg" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "COTA_PCD_ENSINO_SUPERIOR", "Cpf" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "COTA_PCD_ENSINO_SUPERIOR", "Laudo" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "COTA_PCD_ENSINO_SUPERIOR", "Rg" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "CREDENCIAL_IDOSO", "ComprovanteResidencia" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "CREDENCIAL_IDOSO", "Rg" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "CREDENCIAL_PCD", "ComprovanteResidencia" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "CREDENCIAL_PCD", "Laudo" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "CREDENCIAL_PCD", "Rg" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "ISENCAO_IOF_VEICULO_PCD", "Cpf" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "ISENCAO_IOF_VEICULO_PCD", "Laudo" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "ISENCAO_IOF_VEICULO_PCD", "Rg" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "ISENCAO_IPI_VEICULO_PCD", "Cpf" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "ISENCAO_IPI_VEICULO_PCD", "Laudo" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "ISENCAO_IPI_VEICULO_PCD", "Rg" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "ISENCAO_IR_65MAIS", "Cpf" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "ISENCAO_IR_65MAIS", "Rg" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "MEIA_ENTRADA_PCD", "Laudo" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "MEIA_ENTRADA_PCD", "Rg" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "PASSE_IDOSO_INTERESTADUAL", "ComprovanteRenda" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "PASSE_IDOSO_INTERESTADUAL", "Rg" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "PRIORIDADE_IDOSO", "Rg" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "PRIORIDADE_PCD", "Laudo" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "PRIORIDADE_PCD", "Rg" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "TARIFA_SOCIAL_ENERGIA", "ComprovanteResidencia" });

            migrationBuilder.DeleteData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                keyColumns: new[] { "DireitoCodigo", "TipoDocumento" },
                keyValues: new object[] { "TARIFA_SOCIAL_ENERGIA", "Cpf" });

            migrationBuilder.InsertData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                columns: new[] { "DireitoCodigo", "TipoDocumento" },
                values: new object[,]
                {
                    { "FARMACIA_POPULAR", "Cpf" },
                    { "FARMACIA_POPULAR", "Receita" },
                    { "FARMACIA_POPULAR", "Rg" },
                    { "TARIFA_SOCIAL", "ComprovanteResidencia" },
                    { "TARIFA_SOCIAL", "Cpf" }
                });
        }
    }
}
