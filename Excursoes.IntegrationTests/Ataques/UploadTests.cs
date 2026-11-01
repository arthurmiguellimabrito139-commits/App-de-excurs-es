using System.Net;
using System.Text;
using System.Text.Json;

namespace Excursoes.IntegrationTests.Ataques;

// Checklist: 9 (storage), 15 (upload protegido)
public class UploadTests : IClassFixture<ApiDeTeste>
{
    private readonly ApiDeTeste _api;

    public UploadTests(ApiDeTeste api)
    {
        _api = api;
    }

    private async Task<HttpResponseMessage> EnviarAsync(byte[] arquivo, string nome, string tipo)
    {
        string admin = await _api.TokenDoAdminAsync();
        HttpRequestMessage pedido = ApiDeTeste.Pedido(HttpMethod.Post, "/api/viagens", admin);
        MultipartFormDataContent form = ApiDeTeste.FormularioDeViagem();
        ApiDeTeste.AdicionarArquivo(form, arquivo, nome, tipo);
        pedido.Content = form;
        return await _api.Cliente.SendAsync(pedido);
    }

    [Fact]
    public async Task ExecutavelDisfarcadoDeJpg_EhRecusado_ENadaFicaSalvo()
    {
        int arquivosAntes = Directory.GetFiles(_api.PastaUploads).Length;
        byte[] executavel = Encoding.ASCII.GetBytes("MZ\u0090\0 isto é um programa, não uma foto");

        HttpResponseMessage resposta = await EnviarAsync(executavel, "foto.jpg", "image/jpeg");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("Envie apenas imagens JPG, PNG ou WEBP.", await ApiDeTeste.MensagemDeErro(resposta));
        Assert.Equal(arquivosAntes, Directory.GetFiles(_api.PastaUploads).Length);
    }

    [Fact]
    public async Task HtmlComScript_EhRecusado()
    {
        byte[] html = Encoding.UTF8.GetBytes("<html><script>alert('roubei seu token')</script></html>");

        HttpResponseMessage resposta = await EnviarAsync(html, "foto.png", "image/png");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task ImagemGrandeDemais_EhRecusada()
    {
        byte[] grande = new byte[6 * 1024 * 1024];
        Array.Copy(ApiDeTeste.PngValido, grande, ApiDeTeste.PngValido.Length);

        HttpResponseMessage resposta = await EnviarAsync(grande, "enorme.png", "image/png");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("Cada imagem pode ter no máximo 5 MB.", await ApiDeTeste.MensagemDeErro(resposta));
    }

    [Fact]
    public async Task ImagemValida_GanhaNomeAleatorio_EEhServidaComSeguranca()
    {
        HttpResponseMessage resposta = await EnviarAsync(ApiDeTeste.PngValido, "../../../minha foto.png", "image/png");
        JsonElement viagem = await ApiDeTeste.LerJson(resposta);
        string url = viagem.GetProperty("imagens")[0].GetProperty("url").GetString()!;

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.DoesNotContain("minha", url);   // o nome original nunca é usado
        Assert.DoesNotContain("..", url);
        Assert.True(url.EndsWith(".png"), url);

        HttpResponseMessage imagem = await _api.Cliente.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, imagem.StatusCode);
        Assert.Equal("image/png", imagem.Content.Headers.ContentType!.MediaType);
        Assert.Equal("nosniff", string.Join("", imagem.Headers.GetValues("X-Content-Type-Options")));
    }

    [Fact]
    public async Task PastaDeUploads_NaoDeixaSairDela()
    {
        HttpResponseMessage tentativa1 = await _api.Cliente.GetAsync("/uploads/../appsettings.json");
        HttpResponseMessage tentativa2 = await _api.Cliente.GetAsync("/uploads/%2e%2e/%2e%2e/schema.sql");

        Assert.Equal(HttpStatusCode.NotFound, tentativa1.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, tentativa2.StatusCode);
    }

    [Fact]
    public async Task ImagemRemovidaNaEdicao_SomeDoDisco()
    {
        string admin = await _api.TokenDoAdminAsync();
        int id = await _api.CriarViagemAsync(admin);
        JsonElement viagem = await ApiDeTeste.LerJson(await _api.Cliente.GetAsync("/api/viagens/" + id));
        JsonElement imagem = viagem.GetProperty("imagens")[0];
        string nomeDoArquivo = Path.GetFileName(new Uri(imagem.GetProperty("url").GetString()!).AbsolutePath);
        Assert.True(File.Exists(Path.Combine(_api.PastaUploads, nomeDoArquivo)));

        HttpRequestMessage editar = ApiDeTeste.Pedido(HttpMethod.Put, "/api/viagens/" + id, admin);
        MultipartFormDataContent form = ApiDeTeste.FormularioDeViagem();
        form.Add(new StringContent(imagem.GetProperty("id").GetInt32().ToString()), "imagensRemover");
        editar.Content = form;
        HttpResponseMessage resposta = await _api.Cliente.SendAsync(editar);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.False(File.Exists(Path.Combine(_api.PastaUploads, nomeDoArquivo)));
    }
}
