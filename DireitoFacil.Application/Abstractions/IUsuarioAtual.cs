namespace DireitoFacil.Application.Abstractions;

/// <summary>
/// Usuário autenticado da requisição atual (vem do JWT emitido pela API Java).
/// A implementação fica na Api, lendo as claims "sub" e "role".
/// </summary>
public interface IUsuarioAtual
{
    /// <summary>Claim "sub": identificador do cidadão, do gestor ou do serviço.</summary>
    string Id { get; }

    /// <summary>Papel ASSISTENTE SOCIAL (GESTOR): enxerga os dados de todos os cidadãos.</summary>
    bool EhGestor { get; }

    /// <summary>Papel SERVICO: a API Java, que atualiza o status da extração.</summary>
    bool EhServico { get; }
}
