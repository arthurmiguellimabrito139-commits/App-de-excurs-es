using System.Net;
using System.Text.Json;

namespace Excursoes.IntegrationTests.Ataques;

// Checklist: 13 (valida tudo no servidor), 14 (limpa o que o usuário manda), concorrência na última vaga
public class ValidacaoECompraTests : IClassFixture<ApiDeTeste>
{
    private readonly ApiDeTeste _api;

    public ValidacaoECompraTests(ApiDeTeste api)
    {
        _api = api;
    }

    [Fact]
    public async Task NumeroDeParcelasAbsurdo_Recusa()
    {
        string admin = await _api.TokenDoAdminAsync();
        int viagem = await _api.CriarViagemAsync(admin);
        string passageiro = await _api.NovoPassageiroAsync();

        HttpResponseMessage cinquenta = await _api.ComprarAsync(passageiro, viagem, "boleto", 50);
        HttpResponseMessage negativo = await _api.ComprarAsync(passageiro, viagem, "boleto", -1);
        HttpResponseMessage pixParcelado = await _api.ComprarAsync(passageiro, viagem, "pix", 2);
        HttpResponseMessage metodoInventado = await _api.ComprarAsync(passageiro, viagem, "bitcoin", 1);

        Assert.Equal(HttpStatusCode.BadRequest, cinquenta.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, negativo.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, pixParcelado.StatusCode);
        Assert.Equal("Pix só aceita pagamento à vista.", await ApiDeTeste.MensagemDeErro(pixParcelado));
        Assert.Equal(HttpStatusCode.BadRequest, metodoInventado.StatusCode);
    }

    [Fact]
    public async Task ViagemComVoltaAntesDaIda_Recusa()
    {
        string admin = await _api.TokenDoAdminAsync();
        HttpRequestMessage pedido = ApiDeTeste.Pedido(HttpMethod.Post, "/api/viagens", admin);
        MultipartFormDataContent form = new MultipartFormDataContent();
        form.Add(new StringContent("Viagem errada"), "titulo");
        form.Add(new StringContent("Goiânia, GO"), "destino");
        form.Add(new StringContent("2027-01-10"), "dataSaida");
        form.Add(new StringContent("2027-01-05"), "dataRetorno");
        form.Add(new StringContent("500"), "valor");
        form.Add(new StringContent("10"), "vagas");
        form.Add(new StringContent("3"), "maxParcelas");
        pedido.Content = form;

        HttpResponseMessage resposta = await _api.Cliente.SendAsync(pedido);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("A volta não pode ser antes da ida.", await ApiDeTeste.MensagemDeErro(resposta));
    }

    [Fact]
    public async Task ValorComPonto_EhEntendidoComoDecimal()
    {
        string admin = await _api.TokenDoAdminAsync();
        int viagem = await _api.CriarViagemAsync(admin, valor: "1890.50");

        JsonElement dados = await ApiDeTeste.LerJson(await _api.Cliente.GetAsync("/api/viagens/" + viagem));

        Assert.Equal(1890.50m, dados.GetProperty("valor").GetDecimal());
    }

    [Fact]
    public async Task JsonQuebrado_DevolveErroSemDetalheTecnico()
    {
        string passageiro = await _api.NovoPassageiroAsync();
        HttpRequestMessage pedido = ApiDeTeste.Pedido(HttpMethod.Post, "/api/viagens/1/comprar", passageiro);
        pedido.Content = new StringContent("{\"metodo\": \"pix\", \"numParcelas\": ", System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage resposta = await _api.Cliente.SendAsync(pedido);
        string corpo = await resposta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("\"erro\"", corpo);
        Assert.DoesNotContain("System.", corpo);
        Assert.DoesNotContain("$.", corpo);
    }

    [Fact]
    public async Task UltimaVaga_DuasComprasAoMesmoTempo_SoUmaPassa()
    {
        string admin = await _api.TokenDoAdminAsync();

        // repete algumas vezes para aumentar a chance de as duas chegarem juntas
        for (int rodada = 0; rodada < 5; rodada++)
        {
            int viagem = await _api.CriarViagemAsync(admin, vagas: 1);
            string passageiroA = await _api.NovoPassageiroAsync();
            string passageiroB = await _api.NovoPassageiroAsync();

            Task<HttpResponseMessage> compraA = _api.ComprarAsync(passageiroA, viagem, "pix", 1);
            Task<HttpResponseMessage> compraB = _api.ComprarAsync(passageiroB, viagem, "pix", 1);
            HttpResponseMessage[] respostas = await Task.WhenAll(compraA, compraB);

            int sucessos = 0;
            foreach (HttpResponseMessage resposta in respostas)
            {
                if (resposta.StatusCode == HttpStatusCode.OK)
                    sucessos++;
                else
                    Assert.True(resposta.StatusCode == HttpStatusCode.Conflict || resposta.StatusCode == HttpStatusCode.BadRequest,
                        "status inesperado: " + resposta.StatusCode);
            }

            JsonElement dados = await ApiDeTeste.LerJson(await _api.Cliente.GetAsync("/api/viagens/" + viagem));
            Assert.Equal(1, sucessos);
            Assert.Equal(0, dados.GetProperty("vagasDisponiveis").GetInt32());
        }
    }

    [Fact]
    public async Task ViagemComVenda_NaoPodeSerExcluida()
    {
        string admin = await _api.TokenDoAdminAsync();
        int viagem = await _api.CriarViagemAsync(admin);
        string passageiro = await _api.NovoPassageiroAsync();
        await _api.ComprarAsync(passageiro, viagem, "pix", 1);

        HttpResponseMessage resposta = await _api.Cliente.SendAsync(
            ApiDeTeste.Pedido(HttpMethod.Delete, "/api/viagens/" + viagem, admin));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _api.Cliente.GetAsync("/api/viagens/" + viagem)).StatusCode);
    }

    [Fact]
    public async Task FluxoCompleto_CompraEPagamentoNaOrdem()
    {
        string admin = await _api.TokenDoAdminAsync();
        int viagem = await _api.CriarViagemAsync(admin, valor: "1000.00");
        string passageiro = await _api.NovoPassageiroAsync();

        JsonElement compra = await ApiDeTeste.LerJson(await _api.ComprarAsync(passageiro, viagem, "boleto", 3));
        JsonElement parcelas = compra.GetProperty("parcelas");
        Assert.Equal("pendente", compra.GetProperty("status").GetString());
        Assert.Equal(1000m, compra.GetProperty("valorTotal").GetDecimal());

        HttpResponseMessage foraDeOrdem = await _api.Cliente.SendAsync(ApiDeTeste.Pedido(HttpMethod.Post,
            "/api/parcelas/" + parcelas[2].GetProperty("id").GetInt32() + "/pagar", passageiro));
        Assert.Equal(HttpStatusCode.BadRequest, foraDeOrdem.StatusCode);

        for (int i = 0; i < 3; i++)
        {
            HttpResponseMessage pagar = await _api.Cliente.SendAsync(ApiDeTeste.Pedido(HttpMethod.Post,
                "/api/parcelas/" + parcelas[i].GetProperty("id").GetInt32() + "/pagar", passageiro));
            Assert.Equal(HttpStatusCode.OK, pagar.StatusCode);
        }

        JsonElement minhas = await ApiDeTeste.LerJson(await _api.Cliente.SendAsync(
            ApiDeTeste.Pedido(HttpMethod.Get, "/api/compras/minhas", passageiro)));
        Assert.Equal("pago", minhas[0].GetProperty("status").GetString());

        JsonElement passageiros = await ApiDeTeste.LerJson(await _api.Cliente.SendAsync(
            ApiDeTeste.Pedido(HttpMethod.Get, "/api/viagens/" + viagem + "/passageiros", admin)));
        Assert.Equal(1000m, passageiros[0].GetProperty("valorPago").GetDecimal());
    }
}
