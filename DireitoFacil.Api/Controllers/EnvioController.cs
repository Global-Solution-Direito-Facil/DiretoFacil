using DireitoFacil.Api.Extensions;
using DireitoFacil.Api.Filters;
using DireitoFacil.Api.Security;
using DireitoFacil.Application.DTOs;
using DireitoFacil.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DireitoFacil.Api.Controllers;

/// <summary>
/// Envios: o pacote de documentos de um pedido de direito, analisado pelo assistente social.
/// Ciclo: Rascunho → Enviado → Aprovado ou Devolvido (corrige e reenvia).
/// </summary>
[ApiController]
[Route("api/envios")]
[Authorize]
[Produces("application/json")]
public class EnvioController(IEnvioService envioService) : ControllerBase
{
    /// <summary>
    /// Cria um envio em rascunho com documentos do próprio cidadão.
    /// </summary>
    /// <param name="request">Caso, código do direito (ex.: CIPTEA) e documentos.</param>
    /// <param name="idempotencyKey">UUID gerado pelo app para esta ação. No Swagger, se vazio, é gerado automaticamente.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpPost]
    [Authorize(Policy = Policies.SomenteCidadao)]
    [Idempotente]
    [EnableRateLimiting(RateLimitPolicies.Escrita)]
    [ProducesResponseType(typeof(EnvioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Criar(
        [FromBody] CriarEnvioRequest request,
        [FromHeader(Name = IdempotenciaFilter.Header)] Guid? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var envio = await envioService.CriarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = envio.Id }, envio);
    }

    /// <summary>
    /// Lista envios. O gestor usa status=Enviado como fila de análise; o cidadão vê os próprios.
    /// </summary>
    /// <param name="filtro">page, pageSize, status e cidadaoId (só gestor).</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpGet]
    [EnableRateLimiting(RateLimitPolicies.Leitura)]
    [ProducesResponseType(typeof(PagedResponse<EnvioResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Listar([FromQuery] EnvioFiltro filtro, CancellationToken cancellationToken)
        => Ok(await envioService.ListarAsync(filtro, cancellationToken));

    /// <summary>
    /// Busca um envio pelo id.
    /// </summary>
    /// <param name="id">Identificador do envio.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpGet("{id:guid}")]
    [EnableRateLimiting(RateLimitPolicies.Leitura)]
    [ProducesResponseType(typeof(EnvioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
        => Ok(await envioService.ObterAsync(id, cancellationToken));

    /// <summary>
    /// Troca os documentos de um envio em Rascunho ou Devolvido.
    /// </summary>
    /// <param name="id">Identificador do envio.</param>
    /// <param name="request">Nova lista de documentos.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.SomenteCidadao)]
    [EnableRateLimiting(RateLimitPolicies.Escrita)]
    [ProducesResponseType(typeof(EnvioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarEnvioRequest request, CancellationToken cancellationToken)
        => Ok(await envioService.AtualizarAsync(id, request, cancellationToken));

    /// <summary>
    /// Envia para análise. Confere os documentos obrigatórios do direito.
    /// </summary>
    /// <param name="id">Identificador do envio.</param>
    /// <param name="idempotencyKey">UUID gerado pelo app para esta ação. No Swagger, se vazio, é gerado automaticamente.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpPost("{id:guid}/submeter")]
    [Authorize(Policy = Policies.SomenteCidadao)]
    [Idempotente]
    [EnableRateLimiting(RateLimitPolicies.Escrita)]
    [ProducesResponseType(typeof(EnvioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Submeter(
        Guid id,
        [FromHeader(Name = IdempotenciaFilter.Header)] Guid? idempotencyKey,
        CancellationToken cancellationToken)
        => Ok(await envioService.SubmeterAsync(id, cancellationToken));

    /// <summary>
    /// Aprova ou devolve um envio (assistente social). Devolver exige observação.
    /// </summary>
    /// <param name="id">Identificador do envio.</param>
    /// <param name="request">Decisão (Aprovar ou Devolver) e observação.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpPost("{id:guid}/analise")]
    [Authorize(Policy = Policies.SomenteGestor)]
    [EnableRateLimiting(RateLimitPolicies.Escrita)]
    [ProducesResponseType(typeof(EnvioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Analisar(Guid id, [FromBody] AnaliseEnvioRequest request, CancellationToken cancellationToken)
        => Ok(await envioService.AnalisarAsync(id, request, cancellationToken));

    /// <summary>
    /// Cancela um envio em Rascunho ou Devolvido.
    /// </summary>
    /// <param name="id">Identificador do envio.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.SomenteCidadao)]
    [EnableRateLimiting(RateLimitPolicies.Escrita)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancelar(Guid id, CancellationToken cancellationToken)
    {
        await envioService.CancelarAsync(id, cancellationToken);
        return NoContent();
    }
}
