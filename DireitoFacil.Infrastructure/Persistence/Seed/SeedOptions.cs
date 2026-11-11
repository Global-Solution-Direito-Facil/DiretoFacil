namespace DireitoFacil.Infrastructure.Persistence.Seed;

/// <summary>
/// Contas criadas na inicialização se ainda não existirem. As senhas vêm de variável de
/// ambiente ou user-secrets, nunca do código.
/// </summary>
public sealed class SeedOptions
{
    public const string Secao = "Seed";

    public ContaSeed? Gestor { get; set; }

    /// <summary>Conta técnica usada pela API Java para atualizar o status dos documentos.</summary>
    public ContaSeed? Servico { get; set; }

    public sealed class ContaSeed
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;

        public bool Valida => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Senha);
    }
}
