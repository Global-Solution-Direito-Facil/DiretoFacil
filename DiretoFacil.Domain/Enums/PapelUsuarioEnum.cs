namespace DiretoFacil.Domain.Enums;

/// <summary>
/// Papéis usados na autorização (claim "role" do JWT).
/// </summary>
public enum PapelUsuarioEnum
{
    /// <summary>Família ou responsável que envia documentos e pedidos.</summary>
    Cidadao,

    /// <summary>Assistente social que analisa os envios.</summary>
    Gestor,

    /// <summary>Outro sistema (a API Java) que atualiza o status da extração.</summary>
    Servico
}
