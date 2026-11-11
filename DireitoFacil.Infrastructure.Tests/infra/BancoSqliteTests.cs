using DireitoFacil.Infrastructure.Persistence;
using DiretoFacil.Domain.Entites;
using DiretoFacil.Domain.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DireitoFacil.Infrastructure.Tests.Infra;

/// <summary>
/// Isso é apenas para demonstração e testes. O banco utilizado em SI é ORACLE
/// Banco SQLite em memória criado a partir do mesmo modelo do EF (configurações + seed).
/// Cada teste recebe um banco novo e vazio. <see cref="NovoContexto"/> abre um contexto
/// "limpo" para conferir o que realmente foi gravado, sem o cache do contexto do teste.
/// </summary>
public sealed class BancoSqlite : IDisposable
{
    private readonly SqliteConnection _conexao = new("DataSource=:memory:");

    public BancoSqlite()
    {
        _conexao.Open();
        using var contexto = NovoContexto();
        contexto.Database.EnsureCreated();
    }

    public DireitoFacilContext NovoContexto()
        => new(new DbContextOptionsBuilder<DireitoFacilContext>().UseSqlite(_conexao).Options);

    public void Dispose() => _conexao.Dispose();

    // ---------- dados de teste ----------

    public const string CidadaoA = "cidadao-a@teste.com";
    public const string CidadaoB = "cidadao-b@teste.com";

    public static Documento Documento(
        string cidadaoId = CidadaoA,
        TipoDocumentoEnum tipo = TipoDocumentoEnum.Rg,
        string? hash = null)
        => new(cidadaoId, tipo, "arquivo.pdf", "application/pdf", 1024,
            hash ?? Guid.NewGuid().ToString("N"), consentimentoSaude: tipo == TipoDocumentoEnum.Laudo);
}