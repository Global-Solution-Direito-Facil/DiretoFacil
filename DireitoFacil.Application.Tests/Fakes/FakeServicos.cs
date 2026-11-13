using DireitoFacil.Application.Abstractions;
using DireitoFacil.Application.DTOs;
using DireitoFacil.Domain.Entities;

namespace DireitoFacil.Application.Tests.Fakes;

internal sealed class FakeUsuarioAtual : IUsuarioAtual
{
    public string Id { get; set; } = Dados.CidadaoA;
    public bool EhGestor { get; set; }
    public bool EhServico { get; set; }

    public static FakeUsuarioAtual Cidadao(string id = Dados.CidadaoA) => new() { Id = id };
    public static FakeUsuarioAtual Gestor() => new() { Id = Dados.Gestor, EhGestor = true };
}

internal sealed class FakeArquivoStorage : IArquivoStorage
{
    public Dictionary<string, byte[]> Arquivos { get; } = [];

    public async Task SalvarAsync(string caminho, Stream conteudo, string contentType, CancellationToken cancellationToken = default)
    {
        using var copia = new MemoryStream();
        await conteudo.CopyToAsync(copia, cancellationToken);
        Arquivos[caminho] = copia.ToArray();
    }

    public Task<Stream> AbrirAsync(string caminho, CancellationToken cancellationToken = default)
        => Task.FromResult<Stream>(new MemoryStream(Arquivos[caminho]));

    public Task ExcluirAsync(string caminho, CancellationToken cancellationToken = default)
    {
        Arquivos.Remove(caminho);
        return Task.CompletedTask;
    }

    public Uri GerarUrlLeitura(string caminho, TimeSpan validade) => new($"https://storage.teste/{caminho}");
}

internal sealed class FakeEventPublisher : IEventPublisher
{
    public List<object> Eventos { get; } = [];

    public void Publicar<TEvento>(TEvento evento) where TEvento : class => Eventos.Add(evento);
}

/// <summary>Hash fake e previsível: só para teste, nunca para produção.</summary>
internal sealed class FakeSenhaHasher : ISenhaHasher
{
    public string GerarHash(string senha) => "hash:" + senha;

    public bool Verificar(string senhaHash, string senha) => senhaHash == "hash:" + senha;
}

internal sealed class FakeTokenService : ITokenService
{
    public TokenResponse Gerar(Usuario usuario)
        => new($"token-{usuario.Id}", DateTime.UtcNow.AddHours(1), UsuarioResponse.FromDomain(usuario));
}
