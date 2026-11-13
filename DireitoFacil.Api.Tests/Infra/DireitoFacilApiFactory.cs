using System.Net.Http.Headers;
using DireitoFacil.Application.Abstractions;
using DireitoFacil.Infrastructure.Persistence;
using DireitoFacil.Domain.Entities;
using DireitoFacil.Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace DireitoFacil.Api.Tests.Infra;

/// <summary>
/// Sobe a API real em memória (mesmo Program.cs, controllers, filtros, JWT, rate limiting e
/// health checks), trocando apenas o Oracle por SQLite em memória e o storage por uma pasta
/// temporária. Cada classe de teste recebe a sua própria instância (IClassFixture).
/// </summary>
public class DireitoFacilApiFactory : WebApplicationFactory<Program>
{
    public const string ChaveJwt = "chave-somente-para-testes-com-mais-de-32-caracteres";
    public const string SenhaPadrao = "senha1234";

    private readonly SqliteConnection _conexao = new("DataSource=:memory:");
    private readonly string _pastaArquivos =
        Path.Combine(Path.GetTempPath(), "direitofacil-api-testes", Guid.NewGuid().ToString("N"));

    public DireitoFacilApiFactory() => _conexao.Open();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Configurações lidas no Program.cs (UseSetting chega antes do builder.Build()).
        builder.UseSetting("ConnectionStrings:DireitoFacilOracle", "nao-usado-nos-testes");
        builder.UseSetting("Database:AplicarMigrations", "false");
        builder.UseSetting("Jwt:Chave", ChaveJwt);
        builder.UseSetting("Storage:Provider", "Local");
        builder.UseSetting("Storage:PastaLocal", _pastaArquivos);
        builder.UseSetting("RabbitMq:Uri", "");

        builder.ConfigureTestServices(services =>
        {
            // Remove o registro do Oracle e coloca SQLite no lugar.
            services.RemoveAll<DbContextOptions<DireitoFacilContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<DireitoFacilContext>>();
            services.AddDbContext<DireitoFacilContext>(options => options.UseSqlite(_conexao));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<DireitoFacilContext>().Database.EnsureCreated();

        return host;
    }

    /// <summary>
    /// Cria um usuário direto no banco e devolve um HttpClient já autenticado com o JWT dele.
    /// Não passa por /api/auth para não consumir o rate limit de login nos outros testes.
    /// </summary>
    public async Task<ClienteAutenticado> CriarClienteAsync(PapelUsuarioEnum papel = PapelUsuarioEnum.Cidadao)
    {
        // Garante que o host (e o banco) já foram criados.
        _ = Server;

        Usuario usuario;
        using (var scope = Services.CreateScope())
        {
            var contexto = scope.ServiceProvider.GetRequiredService<DireitoFacilContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<ISenhaHasher>();

            usuario = new Usuario(
                $"Teste {papel}",
                $"{papel.ToString().ToLowerInvariant()}-{Guid.NewGuid():N}@teste.com",
                hasher.GerarHash(SenhaPadrao),
                papel);

            contexto.Usuarios.Add(usuario);
            await contexto.SaveChangesAsync();
        }

        var token = Services.GetRequiredService<ITokenService>().Gerar(usuario);

        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        return new ClienteAutenticado(client, usuario);
    }

    /// <summary>Executa uma consulta direto no banco, para conferir o que a API gravou.</summary>
    public async Task<T> NoBancoAsync<T>(Func<DireitoFacilContext, Task<T>> consulta)
    {
        using var scope = Services.CreateScope();
        return await consulta(scope.ServiceProvider.GetRequiredService<DireitoFacilContext>());
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        _conexao.Dispose();
        try
        {
            if (Directory.Exists(_pastaArquivos))
                Directory.Delete(_pastaArquivos, recursive: true);
        }
        catch (IOException)
        {
            // Pasta temporária: se algum arquivo ainda estiver aberto, o sistema limpa depois.
        }
    }
}

/// <summary>HttpClient autenticado + o usuário dono do token.</summary>
public sealed record ClienteAutenticado(HttpClient Client, Usuario Usuario);
