using AbrePortas.Domain.Entities;
using DiretoFacil.Domain.Common;
using DiretoFacil.Domain.Enums;
using DiretoFacil.Domain.Expections;

namespace DiretoFacil.Domain.Entites;


/// <summary>
/// Pacote de documentos de um pedido de direito (ex.: CIPTEA), analisado pelo assistente social.
/// Toda a máquina de estados fica aqui
/// </summary>
public class Envio : BaseEntity
{
     private readonly List<EnvioDocumento> _documentos = [];

    public string CidadaoId { get; private set; } = null!;
    public long CasoId { get; private set; }
    public string DireitoCodigo { get; private set; } = null!;
    public StatusEnvioEnum Status { get; private set; } = StatusEnvioEnum.Rascunho;
    public string? Observacao { get; private set; }
    public string? GestorId { get; private set; }
    public DateTime? EnviadoEm { get; private set; }
    public DateTime? AnalisadoEm { get; private set; }

    public IReadOnlyCollection<EnvioDocumento> Documentos => _documentos.AsReadOnly();

    protected Envio()
    {
    }

    public Envio(string cidadaoId, long casoId, string direitoCodigo, IEnumerable<Documento> documentos)
    {
        if (string.IsNullOrWhiteSpace(cidadaoId))
            throw new DomainException("O cidadão é obrigatório.");

        if (casoId <= 0)
            throw new DomainException("O caso informado é inválido.");

        if (string.IsNullOrWhiteSpace(direitoCodigo))
            throw new DomainException("O código do direito é obrigatório.");

        CidadaoId = cidadaoId;
        CasoId = casoId;
        DireitoCodigo = direitoCodigo.Trim().ToUpperInvariant();

        AplicarDocumentos(documentos);
    }

    /// <summary>
    /// Troca os documentos do envio. Só é permitido em RASCUNHO ou DEVOLVIDO.
    /// </summary>
    public void DefinirDocumentos(IEnumerable<Documento> documentos)
    {
        GarantirEditavel();
        AplicarDocumentos(documentos);
        MarcarAtualizado();
    }

    public void Submeter()
    {
        GarantirEditavel();

        if (_documentos.Any(ed => ed.Documento.Status == StatusDocumentoEnum.Falha))
            throw new RegraDeNegocioException("Há documento com falha de leitura. Envie o arquivo novamente.");

        Status = StatusEnvioEnum.Enviado;
        EnviadoEm = DateTime.UtcNow;
        MarcarAtualizado();
    }

    public void Analisar(DecisaoAnaliseEnum decisao, string gestorId, string? observacao)
    {
        switch (decisao)
        {
            case DecisaoAnaliseEnum.Aprovar: Aprovar(gestorId, observacao); break;
            case DecisaoAnaliseEnum.Devolver: Devolver(gestorId, observacao ?? string.Empty); break;
            default: throw new DomainException("Decisão inválida.");
        }
    }

    public void Aprovar(string gestorId, string? observacao = null)
    {
        GarantirEmAnalise(gestorId);

        Status = StatusEnvioEnum.Aprovado;
        RegistrarAnalise(gestorId, observacao);
    }

    public void Devolver(string gestorId, string observacao)
    {
        GarantirEmAnalise(gestorId);

        if (string.IsNullOrWhiteSpace(observacao))
            throw new DomainException("Informe o que a família precisa corrigir.");

        Status = StatusEnvioEnum.Devolvido;
        RegistrarAnalise(gestorId, observacao);
    }

    public void Cancelar()
    {
        if (Status is not (StatusEnvioEnum.Rascunho or StatusEnvioEnum.Devolvido))
            throw new RegraDeNegocioException($"Não é possível cancelar um envio {Status}.");

        Status = StatusEnvioEnum.Cancelado;
        MarcarAtualizado();
    }

    /// <summary>
    /// Envio que ainda "segura" seus documentos (impede excluí-los).
    /// </summary>
    public bool EstaAtivo() => Status != StatusEnvioEnum.Cancelado;

    public bool PertenceA(string cidadaoId) => CidadaoId == cidadaoId;

    private void AplicarDocumentos(IEnumerable<Documento> documentos)
    {
        var lista = (documentos ?? throw new DomainException("Informe os documentos do envio."))
            .DistinctBy(d => d.Id)
            .ToList();

        if (lista.Count == 0)
            throw new DomainException("O envio precisa ter ao menos um documento.");

        if (lista.Any(d => !d.PertenceA(CidadaoId)))
            throw new DomainException("Todos os documentos precisam pertencer ao mesmo cidadão do envio.");

        if (lista.Any(d => d.Status == StatusDocumentoEnum.Falha))
            throw new RegraDeNegocioException("Há documento com falha de leitura. Envie o arquivo novamente.");

        // Diferença em vez de Clear(): evita o EF rastrear duas instâncias com a mesma chave (EnvioId, DocumentoId).
        var novosIds = lista.Select(d => d.Id).ToHashSet();
        _documentos.RemoveAll(ed => !novosIds.Contains(ed.DocumentoId));

        foreach (var documento in lista.Where(d => _documentos.All(ed => ed.DocumentoId != d.Id)))
            _documentos.Add(new EnvioDocumento(Id, documento));
    }

    private void GarantirEditavel()
    {
        if (Status is not (StatusEnvioEnum.Rascunho or StatusEnvioEnum.Devolvido))
            throw new RegraDeNegocioException($"Um envio {Status} não pode ser alterado.");
    }

    private void GarantirEmAnalise(string gestorId)
    {
        if (string.IsNullOrWhiteSpace(gestorId))
            throw new DomainException("O gestor é obrigatório.");

        if (Status != StatusEnvioEnum.Enviado)
            throw new RegraDeNegocioException("Só envios ENVIADOS podem ser analisados.");
    }

    private void RegistrarAnalise(string gestorId, string? observacao)
    {
        GestorId = gestorId;
        Observacao = observacao?.Trim();
        AnalisadoEm = DateTime.UtcNow;
        MarcarAtualizado();
    }
}