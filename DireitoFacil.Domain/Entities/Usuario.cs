using System.Net.Mail;
using DiretoFacil.Domain.Common;
using DiretoFacil.Domain.Enums;
using DiretoFacil.Domain.Exceptions;

namespace DiretoFacil.Domain.Entities;

/// <summary>
/// Conta de acesso à API (login próprio com JWT).
/// A entidade nunca vê a senha em texto: recebe apenas o hash, gerado na Infrastructure.
/// </summary>
public class Usuario : BaseEntity
{
    public string Nome { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string SenhaHash { get; private set; } = null!;
    public PapelUsuarioEnum Papel { get; private set; }

    protected Usuario()
    {
    }

    public Usuario(string nome, string email, string senhaHash, PapelUsuarioEnum papel = PapelUsuarioEnum.Cidadao)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length < 2)
            throw new DomainException("O nome deve ter ao menos 2 caracteres.");

        if (!Enum.IsDefined(papel))
            throw new DomainException("Papel inválido.");

        Nome = nome.Trim();
        Email = NormalizarEmail(email);
        DefinirSenhaHash(senhaHash);
        Papel = papel;
    }

    /// <summary>Troca a senha (recebe o novo hash, nunca a senha).</summary>
    public void AlterarSenha(string novoSenhaHash)
    {
        DefinirSenhaHash(novoSenhaHash);
        MarcarAtualizado();
    }

    /// <summary>
    /// E-mail sempre em minúsculas e sem espaços, para que "Ana@Mail.com" e "ana@mail.com"
    /// sejam a mesma conta.
    /// </summary>
    public static string NormalizarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("O e-mail é obrigatório.");

        var normalizado = email.Trim().ToLowerInvariant();

        if (normalizado.Length > 254 || !MailAddress.TryCreate(normalizado, out var endereco) || endereco.Address != normalizado)
            throw new DomainException("E-mail inválido.");

        return normalizado;
    }

    private void DefinirSenhaHash(string senhaHash)
    {
        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new DomainException("O hash da senha é obrigatório.");

        SenhaHash = senhaHash;
    }
}
