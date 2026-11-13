namespace DireitoFacil.Domain.Common;

/// <summary>
/// Base de todas as entidades: identidade e datas de auditoria.
/// O Id e o CriadoEm nascem com a entidade; setters privados impedem que sejam alterados de fora.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    public DateTime? AtualizadoEm { get; private set; }

    protected void MarcarAtualizado() => AtualizadoEm = DateTime.UtcNow;
}
