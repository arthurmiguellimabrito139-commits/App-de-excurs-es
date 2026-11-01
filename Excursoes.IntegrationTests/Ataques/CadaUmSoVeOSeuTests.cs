using System.Net;
using System.Text.Json;

namespace Excursoes.IntegrationTests.Ataques;

// Checklist: 6 (não confia no ID da tela), 7 (cada um só vê o seu)
public class CadaUmSoVeOSeuTests : IClassFixture<ApiDeTeste>
{
    private readonly ApiDeTeste _api;

    public CadaUmSoVeOSeuTests(ApiDeTeste api)
    {
        _api = api;
    }

    [Fact]
    public async Task OutroAdmin_NaoEditaNaoExcluiNemVePassageiros_DaViagemAlheia()
    {
        string adminA = await _api.TokenDoAdminAsync();
        string adminB = await _api.NovoAdminAsync();
        int viagemDoA = await _api.CriarViagemAsync(adminA);

        HttpRequestMessage editar = ApiDeTeste.Pedido(HttpMethod.Put, "/api/viagens/" + viagemDoA, adminB);
        editar.Content = ApiDeTeste.FormularioDeViagem(valor: "1.00");
        HttpResponseMessage respostaEditar = await _api.Cliente.SendAsync(editar);

        HttpResponseMessage respostaExcluir = await _api.Cliente.SendAsync(
            ApiDeTeste.Pedido(HttpMethod.Delete, "/api/viagens/" + viagemDoA, adminB));
        HttpResponseMessage respostaPassageiros = await _api.Cliente.SendAsync(
            ApiDeTeste.Pedido(HttpMethod.Get, "/api/viagens/" + viagemDoA + "/passageiros", adminB));

        // 404, igual a uma viagem que não existe: não confirma nem que ela existe
        Assert.Equal(HttpStatusCode.NotFound, respostaEditar.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, respostaExcluir.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, respostaPassageiros.StatusCode);

        // e a viagem continua intacta
        JsonElement viagem = await ApiDeTeste.LerJson(await _api.Cliente.GetAsync("/api/viagens/" + viagemDoA));
        Assert.Equal(1000m, viagem.GetProperty("valor").GetDecimal());
    }

    [Fact]
    public async Task Passageiro_SoVeAsProprias_Compras()
    {
        string admin = await _api.TokenDoAdminAsync();
        int viagem = await _api.CriarViagemAsync(admin);
        string passageiroA = await _api.NovoPassageiroAsync();
        string passageiroB = await _api.NovoPassageiroAsync();

        HttpResponseMessage compra = await _api.ComprarAsync(passageiroA, viagem, "boleto", 2);
        Assert.Equal(HttpStatusCode.OK, compra.StatusCode);

        HttpResponseMessage doB = await _api.Cliente.SendAsync(
            ApiDeTeste.Pedido(HttpMethod.Get, "/api/compras/minhas", passageiroB));
        JsonElement listaDoB = await ApiDeTeste.LerJson(doB);

        Assert.Equal(HttpStatusCode.OK, doB.StatusCode);
        Assert.Equal(0, listaDoB.GetArrayLength());
    }

    [Fact]
    public async Task Passageiro_NaoPagaParcela_DeOutraPessoa()
    {
        string admin = await _api.TokenDoAdminAsync();
        int viagem = await _api.CriarViagemAsync(admin);
        string passageiroA = await _api.NovoPassageiroAsync();
        string passageiroB = await _api.NovoPassageiroAsync();

        JsonElement compraDoA = await ApiDeTeste.LerJson(await _api.ComprarAsync(passageiroA, viagem, "boleto", 2));
        int parcelaDoA = compraDoA.GetProperty("parcelas")[0].GetProperty("id").GetInt32();

        HttpResponseMessage tentativa = await _api.Cliente.SendAsync(
            ApiDeTeste.Pedido(HttpMethod.Post, "/api/parcelas/" + parcelaDoA + "/pagar", passageiroB));

        Assert.Equal(HttpStatusCode.NotFound, tentativa.StatusCode);
        Assert.Equal("Parcela não encontrada.", await ApiDeTeste.MensagemDeErro(tentativa));
    }

    [Fact]
    public async Task Compra_IgnoraValorEIdMandadosPelaTela()
    {
        string admin = await _api.TokenDoAdminAsync();
        int viagem = await _api.CriarViagemAsync(admin, valor: "1890.00");
        string passageiro = await _api.NovoPassageiroAsync();

        // O atacante tenta mandar o próprio valor e o id de outra pessoa
        HttpRequestMessage pedido = ApiDeTeste.Pedido(HttpMethod.Post, "/api/viagens/" + viagem + "/comprar", passageiro);
        pedido.Content = new StringContent(
            "{\"metodo\":\"boleto\",\"numParcelas\":1,\"valor\":1,\"valorTotal\":1,\"passageiroId\":1}",
            System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage resposta = await _api.Cliente.SendAsync(pedido);
        JsonElement compra = await ApiDeTeste.LerJson(resposta);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal(1890m, compra.GetProperty("valorTotal").GetDecimal());

        // e a compra ficou com quem está no token, não com o id 1 (o admin)
        HttpResponseMessage minhas = await _api.Cliente.SendAsync(
            ApiDeTeste.Pedido(HttpMethod.Get, "/api/compras/minhas", passageiro));
        Assert.Equal(1, (await ApiDeTeste.LerJson(minhas)).GetArrayLength());
    }
}
