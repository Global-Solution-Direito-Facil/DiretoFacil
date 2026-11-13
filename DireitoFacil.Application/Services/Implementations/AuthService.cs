using DireitoFacil.Application.Abstractions;
using DireitoFacil.Application.DTOs;
using DireitoFacil.Application.Exceptions;
using DireitoFacil.Application.Repositories;
using DireitoFacil.Application.Repositories.Interfaces;
using DireitoFacil.Application.Services.Interfaces;
using DireitoFacil.Domain.Entities;
using DireitoFacil.Domain.Enums;
using DireitoFacil.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace DireitoFacil.Application.Services.Implementations;

/// <summary>
/// Cadastro de cidadãos e emissão de JWT.
/// </summary>
public sealed class AuthService(
    IUsuarioRepository usuarioRepository,
    ISenhaHasher senhaHasher,
    ITokenService tokenService,
    IUnitOfWork unitOfWork,
    ILogger<AuthService> logger) : IAuthService
{
    /// <inheritdoc />
    public async Task<UsuarioResponse> RegistrarAsync(
        RegistrarUsuarioRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = Usuario.NormalizarEmail(request.Email);

        if (await usuarioRepository.ExisteEmailAsync(email, cancellationToken))
            throw new RegraDeNegocioException("Já existe uma conta com este e-mail.");

        // Todo cadastro pelo app é de cidadão; o papel nunca vem do cliente.
        var usuario = new Usuario(request.Nome, email, senhaHasher.GerarHash(request.Senha), PapelUsuarioEnum.Cidadao);

        usuarioRepository.Add(usuario);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Usuário {UsuarioId} cadastrado", usuario.Id);
        return UsuarioResponse.FromDomain(usuario);
    }

    /// <inheritdoc />
    public async Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        Usuario? usuario = null;
        if (!string.IsNullOrWhiteSpace(request.Email) && !string.IsNullOrEmpty(request.Senha))
        {
            var email = request.Email.Trim().ToLowerInvariant();
            usuario = await usuarioRepository.GetByEmailAsync(email, cancellationToken);
        }

        // Mesma resposta para e-mail inexistente e senha errada: não revela quem tem conta.
        if (usuario is null || !senhaHasher.Verificar(usuario.SenhaHash, request.Senha))
        {
            logger.LogWarning("Tentativa de login inválida");
            throw new CredenciaisInvalidasException();
        }

        logger.LogInformation("Usuário {UsuarioId} autenticado", usuario.Id);
        return tokenService.Gerar(usuario);
    }
}
