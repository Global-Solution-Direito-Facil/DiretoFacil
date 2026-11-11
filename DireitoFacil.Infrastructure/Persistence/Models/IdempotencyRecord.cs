namespace DireitoFacil.Infrastructure.Persistence.Models;

/// <summary>
/// Registro de uma requisição com Idempotency-Key. Se o app reenviar a mesma requisição
/// (mesma chave, mesmo usuário, mesmo endpoint), a API devolve a resposta guardada em vez
/// de executar de novo. É detalhe técnico, por isso fica na Infrastructure e não no Domain.
/// </summary>
public class IdempotencyRecord
{
    public Guid Chave { get; private set; }
    public string UsuarioId { get; private set; } = null!;

    /// <summary>Método + rota, ex.: "POST /api/documentos".</summary>
    public string Endpoint { get; private set; } = null!;

    /// <summary>SHA-256 do conteúdo da requisição: mesma chave com conteúdo diferente é recusada.</summary>
    public string Fingerprint { get; private set; } = null!;

    /// <summary>Nulo enquanto a primeira requisição ainda está em processamento.</summary>
    public int? StatusCode { get; private set; }

    public string? RespostaJson { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime ExpiraEm { get; private set; }

    protected IdempotencyRecord()
    {
    }

    public IdempotencyRecord(Guid chave, string usuarioId, string endpoint, string fingerprint, TimeSpan validade)
    {
        Chave = chave;
        UsuarioId = usuarioId;
        Endpoint = endpoint;
        Fingerprint = fingerprint;
        CriadoEm = DateTime.UtcNow;
        ExpiraEm = CriadoEm.Add(validade);
    }

    public bool EstaConcluido => StatusCode is not null;

    public bool EstaExpirado(DateTime agora) => agora >= ExpiraEm;

    public void Concluir(int statusCode, string? respostaJson)
    {
        StatusCode = statusCode;
        RespostaJson = respostaJson;
    }
}
