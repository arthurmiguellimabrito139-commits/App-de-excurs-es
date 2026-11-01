using System.Net;

namespace Excursoes.IntegrationTests.Ataques;

// Checklist: 11 (debug desligado), 12 (erro sem detalhe), 19 (headers e CORS certos)
public class CabecalhosECorsTests : IClassFixture<ApiDeTeste>
{
    private readonly ApiDeTeste _api;

    public CabecalhosECorsTests(ApiDeTeste api)
    {
        _api = api;
    }

    [Fact]
    public async Task Respostas_TemCabecalhosDeSeguranca_ENaoAnunciamOServidor()
    {
        HttpResponseMessage resposta = await _api.Cliente.GetAsync("/api/viagens");

        Assert.Equal("nosniff", string.Join("", resposta.Headers.GetValues("X-Content-Type-Options")));
        Assert.Equal("DENY", string.Join("", resposta.Headers.GetValues("X-Frame-Options")));
        Assert.Equal("no-referrer", string.Join("", resposta.Headers.GetValues("Referrer-Policy")));
        Assert.Contains("frame-ancestors 'none'", string.Join("", resposta.Headers.GetValues("Content-Security-Policy")));
        Assert.False(resposta.Headers.Contains("Server"), "não deve dizer qual servidor é");
    }

    [Fact]
    public async Task Cors_SiteDesconhecido_NaoRecebePermissao()
    {
        HttpRequestMessage pedido = new HttpRequestMessage(HttpMethod.Options, "/api/compras/minhas");
        pedido.Headers.Add("Origin", "https://site-malicioso.com");
        pedido.Headers.Add("Access-Control-Request-Method", "GET");
        pedido.Headers.Add("Access-Control-Request-Headers", "authorization");

        HttpResponseMessage resposta = await _api.Cliente.SendAsync(pedido);

        Assert.False(resposta.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Cors_Front_RecebePermissao()
    {
        HttpRequestMessage pedido = new HttpRequestMessage(HttpMethod.Options, "/api/compras/minhas");
        pedido.Headers.Add("Origin", ApiDeTeste.OrigemDoFront);
        pedido.Headers.Add("Access-Control-Request-Method", "GET");
        pedido.Headers.Add("Access-Control-Request-Headers", "authorization");

        HttpResponseMessage resposta = await _api.Cliente.SendAsync(pedido);

        Assert.Equal(ApiDeTeste.OrigemDoFront, string.Join("", resposta.Headers.GetValues("Access-Control-Allow-Origin")));
    }

    [Fact]
    public async Task RotaInexistente_NaoMostraDetalhes()
    {
        HttpResponseMessage resposta = await _api.Cliente.GetAsync("/api/rota-que-nao-existe");
        string corpo = await resposta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.DoesNotContain("Exception", corpo);
        Assert.DoesNotContain("/home", corpo);
        Assert.DoesNotContain("C:\\", corpo);
    }

    [Fact]
    public async Task CorpoGiganteForaDoUpload_EhRecusado()
    {
        string lixo = new string('a', 2 * 1024 * 1024);
        StringContent corpo = new StringContent("{\"email\":\"" + lixo + "\",\"senha\":\"x\"}",
            System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage resposta = await _api.Cliente.PostAsync("/api/auth/login", corpo);

        Assert.True(resposta.StatusCode == HttpStatusCode.RequestEntityTooLarge || resposta.StatusCode == HttpStatusCode.BadRequest,
            "status: " + resposta.StatusCode);
    }

    [Fact]
    public async Task NaoExistemTelasDeDebugOuDocumentacaoAbertas()
    {
        string[] rotas = new string[] { "/swagger", "/swagger/index.html", "/openapi/v1.json", "/.env", "/appsettings.json" };
        foreach (string rota in rotas)
        {
            HttpResponseMessage resposta = await _api.Cliente.GetAsync(rota);
            Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        }
    }
}
