namespace DireitoFacil.Api.Extensions;

/// <summary>
/// Script executado pelo Swagger UI antes de cada requisição. Em POSTs (exceto login e
/// cadastro) sem o header Idempotency-Key, gera um UUID novo. Não altera a regra da API.
/// </summary>
/// <remarks>
/// O script PRECISA ficar em uma única linha: o Swashbuckle o embute numa string JavaScript
/// e quebras de linha fazem a página do Swagger falhar ao carregar (tela em branco).
/// Por isso ele é montado por concatenação, sem "\n".
/// </remarks>
public static class SwaggerIdempotencia
{
    public const string Interceptor =
        "(req) => { " +
            "var metodo = (req.method || '').toUpperCase(); " +
            "if (metodo !== 'POST' || (req.url || '').indexOf('/api/auth/') >= 0) { return req; } " +
            "req.headers = req.headers || {}; " +
            "var jaTem = Object.keys(req.headers).some(function (h) { " +
                "return h.toLowerCase() === 'idempotency-key' && String(req.headers[h]).trim() !== ''; }); " +
            "if (!jaTem) { " +
                "var uuid = (window.crypto && window.crypto.randomUUID) ? window.crypto.randomUUID() : " +
                    "'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) { " +
                        "var r = Math.random() * 16 | 0; return (c === 'x' ? r : (r & 3) | 8).toString(16); }); " +
                "req.headers['Idempotency-Key'] = uuid; " +
                "console.log('Idempotency-Key gerada automaticamente: ' + uuid); " +
            "} " +
            "return req; " +
        "}";
}
