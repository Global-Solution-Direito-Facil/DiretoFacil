using DireitoFacil.Application.Abstractions;
using DireitoFacil.Application.DTOs;
using DireitoFacil.Application.Repositories;
using DireitoFacil.Application.Repositories.Interfaces;
using DireitoFacil.Application.Services.Interfaces;
using DireitoFacil.Application.Validation;
using DiretoFacil.Domain.Entities;
using DiretoFacil.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace DireitoFacil.Application.Services.Implementations;

/// <summary>
/// Orquestra a montagem, o envio e a análise dos pacotes de documentos.
/// A máquina de estados fica na entidade <see cref="Envio"/>.
/// </summary>
public sealed class EnvioService(
    IEnvioRepository envioRepository,
    IDocumentoRepository documentoRepository,
    IDocumentoObrigatorioRepository documentoObrigatorioRepository,
    IUnitOfWork unitOfWork,
    IUsuarioAtual usuarioAtual,
    ILogger<EnvioService> logger) : IEnvioService
{
    /// <inheritdoc />
    public async Task<EnvioResponse> CriarAsync(CriarEnvioRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var direitoCodigo = DocumentoObrigatorio.NormalizarCodigo(request.DireitoCodigo);
        if (!await documentoObrigatorioRepository.DireitoExisteAsync(direitoCodigo, cancellationToken))
            throw new DomainException($"Direito '{direitoCodigo}' não encontrado no catálogo.");

        var documentos = await ObterDocumentosDoUsuarioAsync(request.DocumentoIds, cancellationToken);
        var envio = new Envio(usuarioAtual.Id, request.CasoId, direitoCodigo, documentos);

        envioRepository.Add(envio);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Envio {EnvioId} criado para o direito {Direito} com {Quantidade} documento(s)",
            envio.Id, envio.DireitoCodigo, envio.Documentos.Count);

        return EnvioResponse.FromDomain(envio);
    }

    /// <inheritdoc />
    public async Task<PagedResponse<EnvioResponse>> ListarAsync(EnvioFiltro filtro, CancellationToken cancellationToken = default)
    {
        PaginacaoValidator.Validar(filtro.Page, filtro.PageSize);

        var filtroEfetivo = usuarioAtual.EhGestor ? filtro : filtro with { CidadaoId = usuarioAtual.Id };

        var (items, totalItems) = await envioRepository.GetPagedAsync(filtroEfetivo, cancellationToken);

        return PagedResponse<EnvioResponse>.Create(
            items.Select(EnvioResponse.FromDomain).ToList(), filtro.Page, filtro.PageSize, totalItems);
    }

    /// <inheritdoc />
    public async Task<EnvioResponse> ObterAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var envio = await envioRepository.ObterComDocumentosAsync(id, cancellationToken);

        if (envio is null || !(envio.PertenceA(usuarioAtual.Id) || usuarioAtual.EhGestor))
            throw new KeyNotFoundException("Envio não encontrado.");

        return EnvioResponse.FromDomain(envio);
    }

    /// <inheritdoc />
    public async Task<EnvioResponse> AtualizarAsync(
        Guid id, AtualizarEnvioRequest request, CancellationToken cancellationToken = default)
    {
        var envio = await ObterDoUsuarioAsync(id, cancellationToken);
        var documentos = await ObterDocumentosDoUsuarioAsync(request.DocumentoIds, cancellationToken);

        envio.DefinirDocumentos(documentos);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return EnvioResponse.FromDomain(envio);
    }

    /// <inheritdoc />
    public async Task<EnvioResponse> SubmeterAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var envio = await ObterDoUsuarioAsync(id, cancellationToken);
        var obrigatorios = await documentoObrigatorioRepository.ObterTiposAsync(envio.DireitoCodigo, cancellationToken);

        envio.Submeter(obrigatorios);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Envio {EnvioId} submetido para análise", envio.Id);
        return EnvioResponse.FromDomain(envio);
    }

    /// <inheritdoc />
    public async Task<EnvioResponse> AnalisarAsync(
        Guid id, AnaliseEnvioRequest request, CancellationToken cancellationToken = default)
    {
        if (!usuarioAtual.EhGestor)
            throw new UnauthorizedAccessException("Apenas o assistente social pode analisar envios.");

        var envio = await envioRepository.ObterComDocumentosAsync(id, cancellationToken)
                    ?? throw new KeyNotFoundException("Envio não encontrado.");

        envio.Analisar(request.Decisao, usuarioAtual.Id, request.Observacao);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Envio {EnvioId} analisado: {Status}", envio.Id, envio.Status);
        return EnvioResponse.FromDomain(envio);
    }

    /// <inheritdoc />
    public async Task CancelarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var envio = await ObterDoUsuarioAsync(id, cancellationToken);

        envio.Cancelar();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Envio> ObterDoUsuarioAsync(Guid id, CancellationToken cancellationToken)
    {
        var envio = await envioRepository.ObterComDocumentosAsync(id, cancellationToken);

        if (envio is null || !envio.PertenceA(usuarioAtual.Id))
            throw new KeyNotFoundException("Envio não encontrado.");

        return envio;
    }

    /// <summary>
    /// Busca só entre os documentos do próprio cidadão: um id de outra pessoa aparece como
    /// "não encontrado", sem revelar que existe.
    /// </summary>
    private async Task<IReadOnlyList<Documento>> ObterDocumentosDoUsuarioAsync(
        IReadOnlyList<Guid> ids, CancellationToken cancellationToken)
    {
        var idsDistintos = (ids ?? []).Distinct().ToList();
        var documentos = await documentoRepository.ObterDoCidadaoAsync(usuarioAtual.Id, idsDistintos, cancellationToken);

        var faltando = idsDistintos.Except(documentos.Select(d => d.Id)).ToList();
        if (faltando.Count > 0)
            throw new KeyNotFoundException($"Documento(s) não encontrado(s): {string.Join(", ", faltando)}.");

        return documentos;
    }
}
