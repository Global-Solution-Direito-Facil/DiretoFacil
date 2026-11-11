using System.Data.Common;
using DireitoFacil.Application.Exceptions;
using DiretoFacil.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DireitoFacil.Api.Exceptions;

/// <summary>
/// Converte qualquer exceção não tratada em application/problem+json, evitando try/catch
/// espalhado pelos controllers. A ordem do switch importa: exceções mais específicas primeiro.
/// </summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = MapException(exception);

        if (statusCode >= 500)
            logger.LogError(exception, "Erro não tratado. TraceId {TraceId}", httpContext.TraceIdentifier);
        else
            logger.LogInformation("Requisição recusada ({Status}): {Mensagem}", statusCode, exception.Message);

        httpContext.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Type = "about:blank",
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        if (environment.IsDevelopment())
            problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        // contentType no próprio WriteAsJsonAsync: senão ele sobrescreve com application/json.
        await httpContext.Response.WriteAsJsonAsync(
            problem, options: null, contentType: "application/problem+json", cancellationToken: cancellationToken);
        return true;
    }

    private static (int StatusCode, string Title, string? Detail) MapException(Exception exception) => exception switch
    {
        CredenciaisInvalidasException e => (StatusCodes.Status401Unauthorized, "Não autenticado", e.Message),
        UnauthorizedAccessException e => (StatusCodes.Status403Forbidden, "Acesso negado", e.Message),
        RegraDeNegocioException e => (StatusCodes.Status409Conflict, "Operação não permitida no estado atual", e.Message),
        DomainException e => (StatusCodes.Status400BadRequest, "Dados inválidos", e.Message),
        KeyNotFoundException e => (StatusCodes.Status404NotFound, "Recurso não encontrado", e.Message),
        ArgumentException e => (StatusCodes.Status400BadRequest, "Requisição inválida", e.Message),
        BadHttpRequestException e => (e.StatusCode, "Requisição inválida", e.Message),
        _ when EhViolacaoDeUnicidade(exception) => (
            StatusCodes.Status409Conflict, "Registro duplicado", "Já existe um registro com estes dados."),
        DbException or { InnerException: DbException } => (
            StatusCodes.Status503ServiceUnavailable, "Banco indisponível", "Não foi possível acessar o banco de dados."),
        _ => (
            StatusCodes.Status500InternalServerError,
            "Erro interno do servidor",
            "Ocorreu um erro inesperado. Tente novamente mais tarde.")
    };

    /// <summary>ORA-00001: unique constraint violada (ex.: dois cadastros simultâneos com o mesmo e-mail).</summary>
    private static bool EhViolacaoDeUnicidade(Exception exception)
        => exception.InnerException is DbException db && db.Message.Contains("ORA-00001", StringComparison.Ordinal);
}
