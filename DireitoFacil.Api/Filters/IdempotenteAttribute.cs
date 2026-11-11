using Microsoft.AspNetCore.Mvc;

namespace DireitoFacil.Api.Filters;

/// <summary>
/// Marca uma action como idempotente: exige o header Idempotency-Key e devolve a resposta
/// guardada se a mesma requisição for reenviada (ver <see cref="IdempotenciaFilter"/>).
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class IdempotenteAttribute() : TypeFilterAttribute(typeof(IdempotenciaFilter));
