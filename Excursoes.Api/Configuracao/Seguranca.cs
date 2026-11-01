using System.Net;
using System.Threading.RateLimiting;
using Excursoes.Api.Middlewares;
using Excursoes.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;

namespace Excursoes.Api.Configuracao;

// Tudo que é configuração de segurança fica aqui, num lugar só.
public static class Seguranca
{
    public const string PoliticaCors = "Front";
    public const long LimiteUploadBytes = 60L * 1024 * 1024;   // 10 fotos de 5 MB + folga

    // ==================== Login de verdade: JWT ====================

    public static void AdicionarAutenticacao(IServiceCollection servicos, JwtOpcoes jwt)
    {
        servicos.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(ConfigurarJwt);
        servicos.AddAuthorization();

        void ConfigurarJwt(JwtBearerOptions opcoes)
        {
            // Lê as claims com o nome exato que o TokenService gravou ("id", "role", "nome")
            opcoes.MapInboundClaims = false;

            TokenValidationParameters validacao = new TokenValidationParameters();
            validacao.ValidateIssuer = true;
            validacao.ValidIssuer = jwt.Emissor;
            validacao.ValidateAudience = true;
            validacao.ValidAudience = jwt.Audiencia;
            validacao.ValidateLifetime = true;                       // token vencido é recusado
            validacao.RequireExpirationTime = true;
            validacao.ValidateIssuerSigningKey = true;               // assinatura confere (ninguém altera o token)
            validacao.IssuerSigningKey = jwt.ChaveDeAssinatura();
            validacao.ValidAlgorithms = new string[] { SecurityAlgorithms.HmacSha256 };   // recusa "alg: none" e outros
            validacao.ClockSkew = TimeSpan.FromSeconds(30);
            validacao.NameClaimType = TokenService.ClaimNome;
            validacao.RoleClaimType = TokenService.ClaimPapel;
            opcoes.TokenValidationParameters = validacao;

            JwtBearerEvents eventos = new JwtBearerEvents();
            eventos.OnChallenge = AoExigirLogin;
            eventos.OnForbidden = AoNegarAcesso;
            opcoes.Events = eventos;
        }
    }

    // 401 com mensagem no formato que o front entende
    private static async Task AoExigirLogin(JwtBearerChallengeContext contexto)
    {
        contexto.HandleResponse();
        contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await contexto.Response.WriteAsJsonAsync(new RespostaErro("Faça login para continuar."));
    }

    private static Task AoNegarAcesso(ForbiddenContext contexto)
    {
        contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
        return contexto.Response.WriteAsJsonAsync(new RespostaErro("Você não tem permissão para esta ação."));
    }

    // ==================== CORS: só o endereço do front ====================

    public static void AdicionarCors(IServiceCollection servicos, IConfiguration configuracao)
    {
        string[]? lidas = configuracao.GetSection("Cors:OrigensPermitidas").Get<string[]>();
        if (lidas == null || lidas.Length == 0)
            throw new InvalidOperationException("Cors:OrigensPermitidas precisa ter o endereço do front.");
        string[] origens = lidas;

        servicos.AddCors(ConfigurarCors);

        void ConfigurarCors(CorsOptions opcoes)
        {
            opcoes.AddPolicy(PoliticaCors, ConfigurarPolitica);
        }

        void ConfigurarPolitica(CorsPolicyBuilder politica)
        {
            politica.WithOrigins(origens)                         // nunca AllowAnyOrigin
                    .WithMethods("GET", "POST", "PUT", "DELETE")
                    .WithHeaders("Authorization", "Content-Type");
        }
    }

    // ==================== Limite de tentativas ====================

    public static void AdicionarLimites(IServiceCollection servicos, IConfiguration configuracao)
    {
        int limiteLogin = configuracao.GetValue<int>("Seguranca:LimiteLoginPorMinuto", 5);
        int limiteGeral = configuracao.GetValue<int>("Seguranca:LimiteGeralPorMinuto", 300);
        PoliticaDeLimites politica = new PoliticaDeLimites(limiteLogin, limiteGeral);
        servicos.AddRateLimiter(politica.Configurar);
    }

    // ==================== Validação: resposta { "erro": "..." } ====================

    public static void ConfigurarMvc(MvcOptions opcoes)
    {
        // Mensagens de conversão em português (ex.: "abc" no campo de número)
        DefaultModelBindingMessageProvider mensagens = opcoes.ModelBindingMessageProvider;
        mensagens.SetValueIsInvalidAccessor(ValorInvalido);
        mensagens.SetAttemptedValueIsInvalidAccessor(ValorInvalidoNoCampo);
        mensagens.SetValueMustBeANumberAccessor(ValorInvalido);
        mensagens.SetMissingBindRequiredValueAccessor(ValorInvalido);
        mensagens.SetValueMustNotBeNullAccessor(ValorInvalido);
        mensagens.SetUnknownValueIsInvalidAccessor(ValorInvalido);
        mensagens.SetNonPropertyAttemptedValueIsInvalidAccessor(ValorInvalido);
        mensagens.SetNonPropertyUnknownValueIsInvalidAccessor(DadosInvalidos);
        mensagens.SetNonPropertyValueMustBeANumberAccessor(DadosInvalidos);
        mensagens.SetMissingKeyOrValueAccessor(DadosInvalidos);
        mensagens.SetMissingRequestBodyRequiredValueAccessor(DadosInvalidos);
    }

    public static void ConfigurarRespostaDeValidacao(ApiBehaviorOptions opcoes)
    {
        opcoes.InvalidModelStateResponseFactory = CriarRespostaDeValidacao;
    }

    // Devolve só a primeira mensagem, sem detalhes técnicos do formato JSON
    private static IActionResult CriarRespostaDeValidacao(ActionContext contexto)
    {
        string mensagem = "Dados inválidos.";
        foreach (KeyValuePair<string, ModelStateEntry> item in contexto.ModelState)
        {
            if (item.Value.Errors.Count == 0)
                continue;

            ModelError erro = item.Value.Errors[0];
            bool tecnico = item.Key.StartsWith('$') || erro.Exception != null || string.IsNullOrWhiteSpace(erro.ErrorMessage);
            if (!tecnico)
            {
                mensagem = erro.ErrorMessage;
                break;
            }
        }
        return new BadRequestObjectResult(new RespostaErro(mensagem));
    }

    private static string ValorInvalido(string campo)
    {
        return "Valor inválido no campo " + campo + ".";
    }

    private static string ValorInvalidoNoCampo(string valor, string campo)
    {
        return "Valor inválido no campo " + campo + ".";
    }

    private static string DadosInvalidos()
    {
        return "Dados inválidos.";
    }

    private static string DadosInvalidos(string valor)
    {
        return "Dados inválidos.";
    }

    // ==================== Limites de upload e do servidor ====================

    public static void ConfigurarFormularios(FormOptions opcoes)
    {
        opcoes.MultipartBodyLengthLimit = LimiteUploadBytes;
        opcoes.ValueCountLimit = 200;
    }

    public static void ConfigurarKestrel(KestrelServerOptions opcoes)
    {
        opcoes.AddServerHeader = false;   // não anuncia "Server: Kestrel"
        // Corpo de até 1 MB por padrão; só as rotas de upload (com [RequestSizeLimit]) aceitam mais
        opcoes.Limits.MaxRequestBodySize = 1024 * 1024;
    }

    // ==================== Produção atrás de proxy (Render, etc.) ====================

    public static void ConfigurarProxy(ForwardedHeadersOptions opcoes)
    {
        // Usa o IP real do visitante (para o limite de tentativas) e o https do proxy.
        // ForwardLimit = 1: só confia no último salto, adicionado pelo proxy da hospedagem.
        opcoes.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        opcoes.ForwardLimit = 1;
        opcoes.KnownIPNetworks.Clear();
        opcoes.KnownProxies.Clear();
    }
}

// Limites por IP: X tentativas de login por minuto e Y requisições no geral
public class PoliticaDeLimites
{
    public const string Login = "login";

    private readonly int _limiteLogin;
    private readonly int _limiteGeral;

    public PoliticaDeLimites(int limiteLogin, int limiteGeral)
    {
        _limiteLogin = limiteLogin;
        _limiteGeral = limiteGeral;
    }

    public void Configurar(RateLimiterOptions opcoes)
    {
        opcoes.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        opcoes.OnRejected = AoRejeitar;
        opcoes.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ParticaoGeral);
        opcoes.AddPolicy<string>(Login, ParticaoLogin);
    }

    private RateLimitPartition<string> ParticaoGeral(HttpContext contexto)
    {
        return RateLimitPartition.GetFixedWindowLimiter(Ip(contexto), OpcoesGeral);
    }

    private RateLimitPartition<string> ParticaoLogin(HttpContext contexto)
    {
        return RateLimitPartition.GetFixedWindowLimiter(Ip(contexto), OpcoesLogin);
    }

    private FixedWindowRateLimiterOptions OpcoesGeral(string chave)
    {
        return CriarJanela(_limiteGeral);
    }

    private FixedWindowRateLimiterOptions OpcoesLogin(string chave)
    {
        return CriarJanela(_limiteLogin);
    }

    private static FixedWindowRateLimiterOptions CriarJanela(int limite)
    {
        FixedWindowRateLimiterOptions janela = new FixedWindowRateLimiterOptions();
        janela.PermitLimit = limite;
        janela.Window = TimeSpan.FromMinutes(1);
        janela.QueueLimit = 0;
        return janela;
    }

    private static string Ip(HttpContext contexto)
    {
        IPAddress? ip = contexto.Connection.RemoteIpAddress;
        if (ip == null)
            return "desconhecido";
        return ip.ToString();
    }

    private static async ValueTask AoRejeitar(OnRejectedContext contexto, CancellationToken cancelamento)
    {
        contexto.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await contexto.HttpContext.Response.WriteAsJsonAsync(
            new RespostaErro("Muitas tentativas. Aguarde um minuto e tente de novo."), cancelamento);
    }
}
