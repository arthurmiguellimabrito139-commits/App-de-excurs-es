using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Excursoes.IntegrationTests.Ataques;

// Checklist: 4 (login de verdade), 5 (permissão no servidor), 10 (admin protegido), 16 (sem injeção de SQL)
public class LoginEPermissaoTests : IClassFixture<ApiDeTeste>
{
    private readonly ApiDeTeste _api;

    public LoginEPermissaoTests(ApiDeTeste api)
    {
        _api = api;
    }

    [Fact]
    public async Task SemToken_NaoVeCompras()
    {
        HttpResponseMessage resposta = await _api.Cliente.GetAsync("/api/compras/minhas");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        Assert.Equal("Faça login para continuar.", await ApiDeTeste.MensagemDeErro(resposta));
    }

    [Fact]
    public async Task Passageiro_NaoCriaViagem()
    {
        string tokenPassageiro = await _api.NovoPassageiroAsync();
        HttpRequestMessage pedido = ApiDeTeste.Pedido(HttpMethod.Post, "/api/viagens", tokenPassageiro);
        pedido.Content = ApiDeTeste.FormularioDeViagem();

        HttpResponseMessage resposta = await _api.Cliente.SendAsync(pedido);

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task Admin_NaoCompraPassagem()
    {
        string tokenAdmin = await _api.TokenDoAdminAsync();
        int viagem = await _api.CriarViagemAsync(tokenAdmin);

        HttpResponseMessage resposta = await _api.ComprarAsync(tokenAdmin, viagem, "pix", 1);

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task Cadastro_PedindoParaSerAdmin_CriaPassageiroMesmoAssim()
    {
        HttpResponseMessage resposta = await _api.Cliente.PostAsJsonAsync("/api/auth/cadastro", new
        {
            nome = "Espertinho",
            email = _api.EmailUnico("espertinho"),
            senha = "senha-segura-123",
            tipo = "Admin",
            papel = "Admin"
        });

        JsonElement corpo = await ApiDeTeste.LerJson(resposta);
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("Passageiro", corpo.GetProperty("usuario").GetProperty("tipo").GetString());
    }

    [Fact]
    public async Task Login_SenhaErradaEEmailInexistente_TemAMesmaMensagem()
    {
        HttpResponseMessage senhaErrada = await _api.Cliente.PostAsJsonAsync("/api/auth/login",
            new { email = ApiDeTeste.EmailAdmin, senha = "senha-errada-123" });
        HttpResponseMessage emailInexistente = await _api.Cliente.PostAsJsonAsync("/api/auth/login",
            new { email = "ninguem@teste.com", senha = "senha-errada-123" });

        Assert.Equal(HttpStatusCode.Unauthorized, senhaErrada.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, emailInexistente.StatusCode);
        Assert.Equal(await ApiDeTeste.MensagemDeErro(senhaErrada), await ApiDeTeste.MensagemDeErro(emailInexistente));
    }

    [Fact]
    public async Task Login_ComInjecaoDeSql_NaoEntra()
    {
        HttpResponseMessage resposta = await _api.Cliente.PostAsJsonAsync("/api/auth/login",
            new { email = "' OR '1'='1' -- ", senha = "' OR '1'='1' -- " });

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        Assert.DoesNotContain("token", await resposta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Cadastro_SenhaCurta_Recusa()
    {
        HttpResponseMessage resposta = await _api.Cliente.PostAsJsonAsync("/api/auth/cadastro",
            new { nome = "Fulano", email = _api.EmailUnico("curta"), senha = "123" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("A senha precisa ter entre 8 e 72 caracteres.", await ApiDeTeste.MensagemDeErro(resposta));
    }

    [Fact]
    public async Task TokenAlterado_ParaVirarAdmin_EhRecusado()
    {
        string tokenPassageiro = await _api.NovoPassageiroAsync();

        // Troca "Passageiro" por "Admin" no meio do token (sem saber a chave, a assinatura não confere)
        string[] partes = tokenPassageiro.Split('.');
        string conteudo = Encoding.UTF8.GetString(Base64UrlEncoder.DecodeBytes(partes[1]));
        conteudo = conteudo.Replace("\"Passageiro\"", "\"Admin\"");
        string tokenFalso = partes[0] + "." + Base64UrlEncoder.Encode(conteudo) + "." + partes[2];

        HttpRequestMessage pedido = ApiDeTeste.Pedido(HttpMethod.Post, "/api/viagens", tokenFalso);
        pedido.Content = ApiDeTeste.FormularioDeViagem();
        HttpResponseMessage resposta = await _api.Cliente.SendAsync(pedido);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task TokenAssinadoComOutraChave_EhRecusado()
    {
        string tokenFalso = CriarToken("outra-chave-qualquer-com-mais-de-32-caracteres!!", DateTime.UtcNow.AddHours(1));

        HttpResponseMessage resposta = await _api.Cliente.SendAsync(
            ApiDeTeste.Pedido(HttpMethod.Get, "/api/viagens/1/passageiros", tokenFalso));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task TokenVencido_EhRecusado()
    {
        string tokenVencido = CriarToken(ApiDeTeste.ChaveJwt, DateTime.UtcNow.AddMinutes(-5));

        HttpResponseMessage resposta = await _api.Cliente.SendAsync(
            ApiDeTeste.Pedido(HttpMethod.Get, "/api/viagens/1/passageiros", tokenVencido));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task TokenSemAssinatura_AlgNone_EhRecusado()
    {
        string cabecalho = Base64UrlEncoder.Encode("{\"alg\":\"none\",\"typ\":\"JWT\"}");
        long expira = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        string conteudo = Base64UrlEncoder.Encode(
            "{\"id\":\"1\",\"role\":\"Admin\",\"iss\":\"" + ApiDeTeste.Emissor + "\",\"aud\":\"" + ApiDeTeste.Audiencia + "\",\"exp\":" + expira + "}");
        string tokenSemAssinatura = cabecalho + "." + conteudo + ".";

        HttpResponseMessage resposta = await _api.Cliente.SendAsync(
            ApiDeTeste.Pedido(HttpMethod.Get, "/api/viagens/1/passageiros", tokenSemAssinatura));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    private static string CriarToken(string chave, DateTime expira)
    {
        Claim[] claims = new Claim[] { new Claim("id", "1"), new Claim("role", "Admin") };
        SecurityTokenDescriptor descricao = new SecurityTokenDescriptor();
        descricao.Subject = new ClaimsIdentity(claims);
        descricao.Issuer = ApiDeTeste.Emissor;
        descricao.Audience = ApiDeTeste.Audiencia;
        descricao.NotBefore = expira.AddHours(-2);
        descricao.IssuedAt = expira.AddHours(-2);
        descricao.Expires = expira;
        descricao.SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave)), SecurityAlgorithms.HmacSha256);
        return new JsonWebTokenHandler().CreateToken(descricao);
    }
}
