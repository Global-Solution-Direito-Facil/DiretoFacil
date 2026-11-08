using AbrePortas.Domain.Entities;
using DiretoFacil.Domain.Common;
namespace DiretoFacil.Domain.Entites;

/// <summary>
/// Associação N:N entre Envio e Documento (um mesmo laudo serve para vários pedidos).
/// </summary>
public class EnvioDocumento : BaseEntity
{
    public Guid EnvioId { get; private set; }
    public Guid DocumentoId { get; private set; }
    public Documento Documento { get; private set; } = null!;
    
    protected EnvioDocumento() { }

    internal EnvioDocumento(Guid envioId, Documento documento)
    {
        envioId = envioId;
        DocumentoId = documento.Id;
        Documento = documento;
    }
}