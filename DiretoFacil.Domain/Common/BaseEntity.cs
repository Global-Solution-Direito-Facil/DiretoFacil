namespace DiretoFacil.Domain.Common;

/// <summary>
/// Base de todas as entidades: identidade e datas de auditoria.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    
    public DateTime CriadoEm { get; set; }
    
    public DateTime? AtualizadoEm { get; set; }
    
    protected void MarcarAtualizado() => AtualizadoEm = DateTime.UtcNow;
}