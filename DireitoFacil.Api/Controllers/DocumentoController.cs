using DireitoFacil.Api.Controllers.Forms;
using DireitoFacil.Api.Filters;
using DireitoFacil.Api.Security;
using DireitoFacil.Application.DTOs;
using DireitoFacil.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DireitoFacil.Api.Controllers;

/// <summary>
/// Documentos da família (laudos, receitas, comprovantes). O arquivo fica em storage privado;
/// a API guarda os metadados. Cidadão só acessa os próprios; o gestor (assistente social) vê todos.
/// </summary>
[ApiController]
[Route("api/documentos")]
[Authorize]
[Produces("application/json")]
public class DocumentoController(IDocumentoService documentoService) : ControllerBase
{
    private const long LimiteRequisicaoBytes = 12 * 1024 * 1024; // 10 MB de arquivo + campos do formulário

    /// <summary>
    /// Envia um documento. Idempotente: reenviar com o mesmo Idempotency-Key devolve a mesma resposta.
    /// </summary>
    /// <param name="form">Arquivo e dados do documento.</param>
    /// <param name="idempotencyKey">UUID gerado pelo app para esta ação (o mesmo em caso de reenvio). No Swagger, se vazio, é gerado automaticamente.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    /// <remarks>Limitado a 5 envios em sequência, recarregando 1 a cada 2 minutos por usuário.</remarks>
    [HttpPost]
    [Authorize(Policy = Policies.SomenteCidadao)]
    [Consumes("multipart/form-data")]
    [Idempotente]
    [EnableRateLimiting(RateLimitPolicies.Upload)]
    [RequestSizeLimit(LimiteRequisicaoBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = LimiteRequisicaoBytes)]
    [ProducesResponseType(typeof(DocumentoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Criar(
        [FromForm] DocumentoUploadForm form,
        [FromHeader(Name = IdempotenciaFilter.Header)] Guid? idempotencyKey,
        CancellationToken cancellationToken)
    {
        await using var conteudo = form.Arquivo.OpenReadStream();

        var arquivo = new ArquivoUpload(form.Arquivo.FileName, form.Arquivo.ContentType, form.Arquivo.Length, conteudo);
        var request = new CriarDocumentoRequest(form.Tipo!.Value, form.ConsentimentoSaude, form.Descricao);

        var documento = await documentoService.CriarAsync(request, arquivo, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = documento.Id }, documento);
    }

    /// <summary>
    /// Lista documentos com paginação e filtros. O cidadão vê apenas os próprios.
    /// </summary>
    /// <param name="filtro">page (1+), pageSize (1 a 100), tipo, status e cidadaoId (só gestor).</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpGet]
    [EnableRateLimiting(RateLimitPolicies.Leitura)]
    [ProducesResponseType(typeof(PagedResponse<DocumentoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Listar([FromQuery] DocumentoFiltro filtro, CancellationToken cancellationToken)
        => Ok(await documentoService.ListarAsync(filtro, cancellationToken));

    /// <summary>
    /// Busca um documento pelo id.
    /// </summary>
    /// <param name="id">Identificador do documento.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpGet("{id:guid}")]
    [EnableRateLimiting(RateLimitPolicies.Leitura)]
    [ProducesResponseType(typeof(DocumentoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
        => Ok(await documentoService.ObterAsync(id, cancellationToken));

    /// <summary>
    /// Baixa o arquivo do documento.
    /// </summary>
    /// <param name="id">Identificador do documento.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpGet("{id:guid}/arquivo")]
    [EnableRateLimiting(RateLimitPolicies.Leitura)]
    [Produces("application/octet-stream", "application/json")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BaixarArquivo(Guid id, CancellationToken cancellationToken)
    {
        var arquivo = await documentoService.AbrirArquivoAsync(id, cancellationToken);
        return File(arquivo.Conteudo, arquivo.ContentType, arquivo.NomeArquivo);
    }

    /// <summary>
    /// Corrige tipo e descrição do documento (bloqueado enquanto a IA processa).
    /// </summary>
    /// <param name="id">Identificador do documento.</param>
    /// <param name="request">Novo tipo, descrição e consentimento (necessário para Laudo).</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.SomenteCidadao)]
    [EnableRateLimiting(RateLimitPolicies.Escrita)]
    [ProducesResponseType(typeof(DocumentoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarDocumentoRequest request, CancellationToken cancellationToken)
        => Ok(await documentoService.AtualizarAsync(id, request, cancellationToken));

    /// <summary>
    /// Atualiza o status da extração. Uso exclusivo do serviço de IA (API Java, papel SERVICO).
    /// </summary>
    /// <param name="id">Identificador do documento.</param>
    /// <param name="request">Processando, Extraido ou Falha.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = Policies.SomenteServico)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AtualizarStatus(
        Guid id, [FromBody] AtualizarStatusDocumentoRequest request, CancellationToken cancellationToken)
    {
        await documentoService.AtualizarStatusAsync(id, request, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Exclui o documento e o arquivo (direito de exclusão, LGPD art. 18).
    /// Bloqueado se o documento estiver em um envio ativo.
    /// </summary>
    /// <param name="id">Identificador do documento.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.SomenteCidadao)]
    [EnableRateLimiting(RateLimitPolicies.Escrita)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        await documentoService.ExcluirAsync(id, cancellationToken);
        return NoContent();
    }
}
