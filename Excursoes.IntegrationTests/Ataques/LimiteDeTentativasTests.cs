using System.Net;
using System.Net.Http.Json;

namespace Excursoes.IntegrationTests.Ataques;

// Checklist: 17 (limite de tentativas). Esta API de teste aceita só 3 tentativas de login por minuto.
public class LimiteDeTentativasTests : IClassFixture<ApiComLimiteBaixo>
{
    private readonly ApiComLimiteBaixo _api;

    public LimiteDeTentativasTests(ApiComLimiteBaixo api)
    {
        _api = api;
    }

    [Fact]
    public async Task ForcaBrutaNoLogin_EhBloqueada()
    {
        HttpStatusCode ultimo = HttpStatusCode.OK;
        for (int tentativa = 1; tentativa <= 4; tentativa++)
        {
            HttpResponseMessage resposta = await _api.Cliente.PostAsJsonAsync("/api/auth/login",
                new { email = ApiDeTeste.EmailAdmin, senha = "chute-" + tentativa });
            ultimo = resposta.StatusCode;

            if (tentativa <= 3)
                Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, ultimo);

        // Nem a senha CERTA entra enquanto o bloqueio durar
        HttpResponseMessage senhaCerta = await _api.Cliente.PostAsJsonAsync("/api/auth/login",
            new { email = ApiDeTeste.EmailAdmin, senha = ApiDeTeste.SenhaAdmin });
        Assert.Equal(HttpStatusCode.TooManyRequests, senhaCerta.StatusCode);
        Assert.Equal("Muitas tentativas. Aguarde um minuto e tente de novo.", await ApiDeTeste.MensagemDeErro(senhaCerta));
    }
}
