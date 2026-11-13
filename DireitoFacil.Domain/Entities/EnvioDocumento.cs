namespace DireitoFacil.Domain.Entities;

/// <summary>
/// Associação N:N entre Envio e Documento (um mesmo laudo serve para vários pedidos).
/// Não herda de BaseEntity: a chave é composta (EnvioId, DocumentoId).
/// </summary>
public class EnvioDocumento
{
    public Guid EnvioId { get; private set; }
    public Guid DocumentoId { get; private set; }
    public Documento Documento { get; private set; } = null!;

    protected EnvioDocumento() { }

    internal EnvioDocumento(Guid envioId, Documento documento)
    {
        EnvioId = envioId;
        DocumentoId = documento.Id;
        Documento = documento;
    }
}
