using System.Security.Cryptography;
using DireitoFacil.Application.Abstractions;
using DireitoFacil.Application.DTOs;
using DireitoFacil.Application.Events;
using DireitoFacil.Application.Repositories;
using DireitoFacil.Application.Services.Interfaces;
using DireitoFacil.Application.Validation;
using DiretoFacil.Domain.Entites;
using DiretoFacil.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace DireitoFacil.Application.Services.Implementations;

/// <summary>
/// Orquestra upload, consulta, reclassificação e exclusão de documentos.
/// As regras de estado ficam na entidade <see cref="Documento"/>.
/// </summary>
public sealed class DocumentoService(
    IDocumentoRepository documentoRepository,
    IArquivoStorage arquivoStorage,
    IEventPublisher eventPublisher,
    IUnitOfWork unitOfWork,
    IUsuarioAtual usuarioAtual,
    ILogger<DocumentoService> logger) : IDocumentoService
{
    /// <inheritdoc />
    public async Task<DocumentoResponse> CriarAsync(
        CriarDocumentoRequest request, ArquivoUpload arquivo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(arquivo);

        await using var conteudo = await GarantirStreamPosicionavelAsync(arquivo.Conteudo, cancellationToken);

        ArquivoValidator.ValidarAssinatura(conteudo, arquivo.ContentType);

        var hash = Convert.ToHexString(await SHA256.HashDataAsync(conteudo, cancellationToken));
        conteudo.Position = 0;

        if (await documentoRepository.ExisteHashAsync(usuarioAtual.Id, hash, cancellationToken))
            throw new RegraDeNegocioException("Este arquivo já foi enviado anteriormente.");

        // As validações de tamanho, formato e consentimento acontecem no construtor da entidade.
        var documento = new Documento(
            usuarioAtual.Id,
            request.Tipo,
            arquivo.NomeArquivo,
            arquivo.ContentType,
            arquivo.TamanhoBytes,
            hash,
            request.ConsentimentoSaude,
            request.Descricao);

        await arquivoStorage.SalvarAsync(documento.BlobPath, conteudo, documento.ContentType, cancellationToken);

        documentoRepository.Add(documento);
        eventPublisher.Publicar(new DocumentoRecebidoEvent(
            Guid.NewGuid(),
            documento.Id,
            documento.CidadaoId,
            documento.Tipo.ToString(),
            documento.ContentType,
            documento.BlobPath,
            DateTime.UtcNow));

        try
        {
            // Documento + mensagem do outbox na mesma transação.
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Compensação: não deixa arquivo órfão no storage se o banco falhar.
            await ExcluirArquivoSemFalharAsync(documento.BlobPath);
            throw;
        }

        logger.LogInformation(
            "Documento {DocumentoId} recebido. Tipo: {Tipo}, Tamanho: {Tamanho} bytes",
            documento.Id, documento.Tipo, documento.TamanhoArquivo);

        return DocumentoResponse.FromDomain(documento);
    }

    /// <inheritdoc />
    public async Task<PagedResponse<DocumentoResponse>> ListarAsync(
        DocumentoFiltro filtro, CancellationToken cancellationToken = default)
    {
        PaginacaoValidator.Validar(filtro.Page, filtro.PageSize);

        // Cidadão só lista os próprios documentos, independentemente do filtro enviado.
        var filtroEfetivo = usuarioAtual.EhGestor ? filtro : filtro with { CidadaoId = usuarioAtual.Id };

        var (items, totalItems) = await documentoRepository.GetPagedAsync(filtroEfetivo, cancellationToken);

        return PagedResponse<DocumentoResponse>.Create(
            items.Select(DocumentoResponse.FromDomain).ToList(), filtro.Page, filtro.PageSize, totalItems);
    }

    /// <inheritdoc />
    public async Task<DocumentoResponse> ObterAsync(Guid id, CancellationToken cancellationToken = default)
        => DocumentoResponse.FromDomain(await ObterVisivelAsync(id, cancellationToken));

    /// <inheritdoc />
    public async Task<ArquivoResponse> AbrirArquivoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var documento = await ObterVisivelAsync(id, cancellationToken);
        var conteudo = await arquivoStorage.AbrirAsync(documento.BlobPath, cancellationToken);

        return new ArquivoResponse(conteudo, documento.ContentType, documento.NomeArquivo);
    }

    /// <inheritdoc />
    public async Task<DocumentoResponse> AtualizarAsync(
        Guid id, AtualizarDocumentoRequest request, CancellationToken cancellationToken = default)
    {
        var documento = await ObterDoUsuarioAsync(id, cancellationToken);

        documento.Reclassificar(request.Tipo, request.Descricao, request.ConsentimentoSaude);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return DocumentoResponse.FromDomain(documento);
    }

    /// <inheritdoc />
    public async Task AtualizarStatusAsync(
        Guid id, AtualizarStatusDocumentoRequest request, CancellationToken cancellationToken = default)
    {
        var documento = await documentoRepository.GetByIdAsync(id, cancellationToken)
                        ?? throw new KeyNotFoundException("Documento não encontrado.");

        documento.AtualizarStatus(request.Status);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Documento {DocumentoId} atualizado para {Status}", documento.Id, documento.Status);
    }

    /// <inheritdoc />
    public async Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var documento = await ObterDoUsuarioAsync(id, cancellationToken);

        if (await documentoRepository.EstaEmEnvioAtivoAsync(documento.Id, cancellationToken))
            throw new RegraDeNegocioException(
                "O documento faz parte de um envio. Remova-o do envio ou cancele o envio antes de excluir.");

        documentoRepository.Remove(documento);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // O registro já saiu do banco; uma falha aqui só deixa um arquivo órfão (registrado em log).
        await ExcluirArquivoSemFalharAsync(documento.BlobPath);

        logger.LogInformation("Documento {DocumentoId} excluído a pedido do titular", documento.Id);
    }

    /// <summary>
    /// Dono, gestor ou serviço podem ver. Para os demais, o documento "não existe" (404, não 403),
    /// para não revelar que aquele id é válido.
    /// </summary>
    private async Task<Documento> ObterVisivelAsync(Guid id, CancellationToken cancellationToken)
    {
        var documento = await documentoRepository.GetByIdAsync(id, cancellationToken);

        if (documento is null ||
            !(documento.PertenceA(usuarioAtual.Id) || usuarioAtual.EhGestor || usuarioAtual.EhServico))
            throw new KeyNotFoundException("Documento não encontrado.");

        return documento;
    }

    /// <summary>Só o titular pode alterar ou excluir.</summary>
    private async Task<Documento> ObterDoUsuarioAsync(Guid id, CancellationToken cancellationToken)
    {
        var documento = await documentoRepository.GetByIdAsync(id, cancellationToken);

        if (documento is null || !documento.PertenceA(usuarioAtual.Id))
            throw new KeyNotFoundException("Documento não encontrado.");

        return documento;
    }

    private static async Task<Stream> GarantirStreamPosicionavelAsync(Stream origem, CancellationToken cancellationToken)
    {
        if (origem.CanSeek)
        {
            origem.Position = 0;
            return origem;
        }

        var copia = new MemoryStream();
        await origem.CopyToAsync(copia, cancellationToken);
        copia.Position = 0;
        return copia;
    }

    private async Task ExcluirArquivoSemFalharAsync(string caminho)
    {
        try
        {
            await arquivoStorage.ExcluirAsync(caminho, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Não foi possível excluir o arquivo {Caminho} do storage", caminho);
        }
    }
}
