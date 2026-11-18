using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DireitoFacil.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DocumentoPessoaId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PessoaId",
                table: "DF_DOCUMENTO",
                type: "NUMBER(19)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PessoaId",
                table: "DF_DOCUMENTO");
        }
    }
}
