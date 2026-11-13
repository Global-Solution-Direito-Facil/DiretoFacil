using DireitoFacil.Application.Abstractions;
using DireitoFacil.Domain.Entities;
using DireitoFacil.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DireitoFacil.Infrastructure.Persistence.Seed;

/// <summary>
/// Cria o assistente social (GESTOR) e a conta de serviço na inicialização, pois essas
/// contas não podem ser criadas pelo cadastro público do app.
/// </summary>
public sealed class DatabaseSeeder(
    DireitoFacilContext context,
    ISenhaHasher senhaHasher,
    IOptions<SeedOptions> options,
    ILogger<DatabaseSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await CriarSeNaoExisteAsync(options.Value.Gestor, PapelUsuarioEnum.Gestor, cancellationToken);
        await CriarSeNaoExisteAsync(options.Value.Servico, PapelUsuarioEnum.Servico, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task CriarSeNaoExisteAsync(SeedOptions.ContaSeed? conta, PapelUsuarioEnum papel, CancellationToken cancellationToken)
    {
        if (conta is null || !conta.Valida)
            return;

        var email = Usuario.NormalizarEmail(conta.Email);
        if (await context.Usuarios.AnyAsync(u => u.Email == email, cancellationToken))
            return;

        var nome = string.IsNullOrWhiteSpace(conta.Nome) ? papel.ToString() : conta.Nome;
        context.Usuarios.Add(new Usuario(nome, email, senhaHasher.GerarHash(conta.Senha), papel));
        logger.LogInformation("Conta {Papel} criada pelo seed", papel);
    }
}
