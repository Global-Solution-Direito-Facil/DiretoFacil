using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DireitoFacil.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DF_DOCUMENTO",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CidadaoId = table.Column<string>(type: "NVARCHAR2(254)", maxLength: 254, nullable: false),
                    Tipo = table.Column<string>(type: "NVARCHAR2(40)", maxLength: 40, nullable: false),
                    Descricao = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: true),
                    NomeArquivo = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    TamanhoArquivo = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    BlobPath = table.Column<string>(type: "NVARCHAR2(400)", maxLength: 400, nullable: false),
                    HashSha256 = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    ConsentimentoSaude = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    ConsentimentoSaudeEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DF_DOCUMENTO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DF_DOCUMENTO_OBRIGATORIO",
                columns: table => new
                {
                    DireitoCodigo = table.Column<string>(type: "NVARCHAR2(60)", maxLength: 60, nullable: false),
                    TipoDocumento = table.Column<string>(type: "NVARCHAR2(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DF_DOCUMENTO_OBRIGATORIO", x => new { x.DireitoCodigo, x.TipoDocumento });
                });

            migrationBuilder.CreateTable(
                name: "DF_ENVIO",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CidadaoId = table.Column<string>(type: "NVARCHAR2(254)", maxLength: 254, nullable: false),
                    CasoId = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    DireitoCodigo = table.Column<string>(type: "NVARCHAR2(60)", maxLength: 60, nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    Observacao = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    GestorId = table.Column<string>(type: "NVARCHAR2(254)", maxLength: 254, nullable: true),
                    EnviadoEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    AnalisadoEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DF_ENVIO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DF_IDEMPOTENCIA",
                columns: table => new
                {
                    Chave = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    UsuarioId = table.Column<string>(type: "NVARCHAR2(254)", maxLength: 254, nullable: false),
                    Endpoint = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Fingerprint = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    StatusCode = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    RespostaJson = table.Column<string>(type: "CLOB", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ExpiraEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DF_IDEMPOTENCIA", x => new { x.Chave, x.UsuarioId, x.Endpoint });
                });

            migrationBuilder.CreateTable(
                name: "DF_OUTBOX",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Tipo = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Payload = table.Column<string>(type: "CLOB", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ProcessadoEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    Tentativas = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    UltimoErro = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DF_OUTBOX", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DF_USUARIO",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Nome = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(254)", maxLength: 254, nullable: false),
                    SenhaHash = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Papel = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DF_USUARIO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DF_ENVIO_DOCUMENTO",
                columns: table => new
                {
                    EnvioId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    DocumentoId = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DF_ENVIO_DOCUMENTO", x => new { x.EnvioId, x.DocumentoId });
                    table.ForeignKey(
                        name: "FK_DF_ENVIO_DOCUMENTO_DF_DOCUMENTO_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "DF_DOCUMENTO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DF_ENVIO_DOCUMENTO_DF_ENVIO_EnvioId",
                        column: x => x.EnvioId,
                        principalTable: "DF_ENVIO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DF_DOCUMENTO_OBRIGATORIO",
                columns: new[] { "DireitoCodigo", "TipoDocumento" },
                values: new object[,]
                {
                    { "ACOMPANHANTE_ESCOLAR_TEA", "Laudo" },
                    { "BPC_IDOSO", "ComprovanteRenda" },
                    { "BPC_IDOSO", "ComprovanteResidencia" },
                    { "BPC_IDOSO", "Cpf" },
                    { "BPC_IDOSO", "Rg" },
                    { "BPC_PCD", "ComprovanteRenda" },
                    { "BPC_PCD", "ComprovanteResidencia" },
                    { "BPC_PCD", "Cpf" },
                    { "BPC_PCD", "Laudo" },
                    { "BPC_PCD", "Rg" },
                    { "CADUNICO", "ComprovanteRenda" },
                    { "CADUNICO", "ComprovanteResidencia" },
                    { "CADUNICO", "Cpf" },
                    { "CADUNICO", "Rg" },
                    { "CIPTEA", "Cpf" },
                    { "CIPTEA", "Laudo" },
                    { "CIPTEA", "Rg" },
                    { "FARMACIA_POPULAR", "Cpf" },
                    { "FARMACIA_POPULAR", "Receita" },
                    { "FARMACIA_POPULAR", "Rg" },
                    { "PASSE_LIVRE_PCD", "ComprovanteRenda" },
                    { "PASSE_LIVRE_PCD", "Cpf" },
                    { "PASSE_LIVRE_PCD", "Laudo" },
                    { "PASSE_LIVRE_PCD", "Rg" },
                    { "TARIFA_SOCIAL", "ComprovanteResidencia" },
                    { "TARIFA_SOCIAL", "Cpf" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DF_DOCUMENTO_CidadaoId_CriadoEm",
                table: "DF_DOCUMENTO",
                columns: new[] { "CidadaoId", "CriadoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_DF_DOCUMENTO_CidadaoId_HashSha256",
                table: "DF_DOCUMENTO",
                columns: new[] { "CidadaoId", "HashSha256" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DF_ENVIO_CidadaoId_CriadoEm",
                table: "DF_ENVIO",
                columns: new[] { "CidadaoId", "CriadoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_DF_ENVIO_Status",
                table: "DF_ENVIO",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DF_ENVIO_DOCUMENTO_DocumentoId",
                table: "DF_ENVIO_DOCUMENTO",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_DF_IDEMPOTENCIA_ExpiraEm",
                table: "DF_IDEMPOTENCIA",
                column: "ExpiraEm");

            migrationBuilder.CreateIndex(
                name: "IX_DF_OUTBOX_ProcessadoEm_CriadoEm",
                table: "DF_OUTBOX",
                columns: new[] { "ProcessadoEm", "CriadoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_DF_USUARIO_Email",
                table: "DF_USUARIO",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DF_DOCUMENTO_OBRIGATORIO");

            migrationBuilder.DropTable(
                name: "DF_ENVIO_DOCUMENTO");

            migrationBuilder.DropTable(
                name: "DF_IDEMPOTENCIA");

            migrationBuilder.DropTable(
                name: "DF_OUTBOX");

            migrationBuilder.DropTable(
                name: "DF_USUARIO");

            migrationBuilder.DropTable(
                name: "DF_DOCUMENTO");

            migrationBuilder.DropTable(
                name: "DF_ENVIO");
        }
    }
}
