using DireitoFacil.Application.Abstractions;
using DireitoFacil.Application.Repositories;
using DireitoFacil.Application.Repositories.Interfaces;
using DireitoFacil.Application.Services.Implementations;
using DireitoFacil.Application.Services.Interfaces;
using DireitoFacil.Infrastructure.Messaging;
using DireitoFacil.Infrastructure.Persistence;
using DireitoFacil.Infrastructure.Persistence.Seed;
using DireitoFacil.Infrastructure.Repositories;
using DireitoFacil.Infrastructure.Security;
using DireitoFacil.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DireitoFacil.Api.Extensions;

public static class DireitoFacilServiceCollectionExtensions
{
    public static IServiceCollection AddDireitoFacilDbContext(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        var connectionString = configuration.GetConnectionString("DireitoFacilOracle");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DireitoFacilOracle' não encontrada. Configure com " +
                "'dotnet user-secrets set ConnectionStrings:DireitoFacilOracle \"...\"' (na pasta DireitoFacil.Api) " +
                "ou a variável de ambiente ConnectionStrings__DireitoFacilOracle (Azure).");
        }

        services.AddDbContext<DireitoFacilContext>(options =>
        {
            options.UseOracle(connectionString, oracleOptions =>
            {
                // Mesmo ajuste do projeto de referência: bool vira NUMBER(1) e o provider não gera
                // literais TRUE/FALSE, que não existem no Oracle 19c da FIAP.
                oracleOptions.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19);
            });

            if (env.IsDevelopment())
            {
                options.LogTo(Console.WriteLine, LogLevel.Information);
            }
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<DireitoFacilContext>());

        return services;
    }

    /// <summary>
    /// Repositórios como Scoped (um por requisição, compartilhando o mesmo contexto).
    /// </summary>
    public static IServiceCollection AddDireitoFacilRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IDocumentoRepository, DocumentoRepository>();
        services.AddScoped<IEnvioRepository, EnvioRepository>();
        services.AddScoped<IDocumentoObrigatorioRepository, DocumentoObrigatorioRepository>();

        return services;
    }

    public static IServiceCollection AddDireitoFacilApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDocumentoService, DocumentoService>();
        services.AddScoped<IEnvioService, EnvioService>();

        return services;
    }

    /// <summary>
    /// Hash de senha, storage de arquivos, outbox/RabbitMQ e seed.
    /// </summary>
    public static IServiceCollection AddDireitoFacilInfraestrutura(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ISenhaHasher, SenhaHasher>();

        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.Secao));
        services.AddSingleton<IArquivoStorage>(sp =>
        {
            var storage = sp.GetRequiredService<IOptions<StorageOptions>>();
            return storage.Value.UsaAzureBlob
                ? new BlobArquivoStorage(storage)
                : new LocalArquivoStorage(storage);
        });

        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.Secao));
        services.AddSingleton<RabbitMqPublisher>();
        services.AddScoped<IEventPublisher, OutboxEventPublisher>();
        services.AddHostedService<OutboxWorker>();

        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.Secao));
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
