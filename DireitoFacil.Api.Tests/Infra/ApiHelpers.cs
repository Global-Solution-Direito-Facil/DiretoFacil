using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DireitoFacil.Application.DTOs;
using DiretoFacil.Domain.Enums;

namespace DireitoFacil.Api.Tests.Infra;

/// <summary>
/// Atalhos para montar as requisições dos testes.
/// </summary>
public static class ApiHelpers
{
    public const string HeaderIdempotencia = "Idempotency-Key";
    public const string HeaderReplay = "Idempotent-Replayed";

    /// <summary>Mesmas opções de JSON da API (camelCase + enums como texto).</summary>
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>Um PDF mínimo válido (começa com %PDF). O marcador deixa o conteúdo (e o hash) único.</summary>
    public static byte[] Pdf(string marcador = "") =>
        Encoding.ASCII.GetBytes($"%PDF-1.7\n{marcador}{Guid.NewGuid()}\n%%EOF");

    public static async Task<T> LerAsync<T>(this HttpResponseMessage resposta)
        => (await resposta.Content.ReadFromJsonAsync<T>(Json))!;

    public static async Task<HttpResponseMessage> EnviarDocumentoAsync(
        this HttpClient client,
        TipoDocumentoEnum tipo = TipoDocumentoEnum.Rg,
        byte[]? conteudo = null,
        string contentType = "application/pdf",
        bool consentimentoSaude = false,
        Guid? chave = null,
        bool semChave = false)
    {
        var arquivo = new ByteArrayContent(conteudo ?? Pdf(tipo.ToString()));
        arquivo.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        using var form = new MultipartFormDataContent
        {
            { arquivo, "Arquivo", $"{tipo}.pdf" },
            { new StringContent(tipo.ToString()), "Tipo" },
            { new StringContent(consentimentoSaude ? "true" : "false"), "ConsentimentoSaude" }
        };

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/documentos") { Content = form };
        if (!semChave)
            requisicao.Headers.Add(HeaderIdempotencia, (chave ?? Guid.NewGuid()).ToString());

        return await client.SendAsync(requisicao);
    }

    /// <summary>Envia o documento e devolve a resposta já lida (falha o teste se não for 201).</summary>
    public static async Task<DocumentoResponse> CriarDocumentoAsync(
        this HttpClient client, TipoDocumentoEnum tipo = TipoDocumentoEnum.Rg)
    {
        var resposta = await client.EnviarDocumentoAsync(tipo, consentimentoSaude: tipo == TipoDocumentoEnum.Laudo);
        Assert.Equal(System.Net.HttpStatusCode.Created, resposta.StatusCode);
        return await resposta.LerAsync<DocumentoResponse>();
    }

    public static async Task<HttpResponseMessage> PostJsonAsync(
        this HttpClient client, string url, object? corpo = null, Guid? chave = null, bool semChave = false)
    {
        using var requisicao = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = corpo is null ? null : JsonContent.Create(corpo, options: Json)
        };

        if (!semChave)
            requisicao.Headers.Add(HeaderIdempotencia, (chave ?? Guid.NewGuid()).ToString());

        return await client.SendAsync(requisicao);
    }
}
