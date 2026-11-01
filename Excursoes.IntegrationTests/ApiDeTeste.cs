using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Dapper;
using Excursoes.Api.Configuracao;
using Excursoes.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace Excursoes.IntegrationTests;

// Sobe a API DE VERDADE (mesmo código do Program.cs) numa porta aleatória,
// com um banco MySQL criado só para o teste e apagado no final.
//
// Precisa da variável de ambiente EXCURSOES_TESTE_MYSQL com um usuário que possa criar bancos, ex.:
//   PowerShell:  $env:EXCURSOES_TESTE_MYSQL = "server=localhost;user=root;password=SUA_SENHA"
public class ApiDeTeste : IAsyncLifetime
{
    public const string EmailAdmin = "admin@teste.com";
    public const string SenhaAdmin = "SenhaDoAdminDeTeste2026";
    public const string ChaveJwt = "chave-de-teste-com-bem-mais-de-32-caracteres-0123456789";
    public const string Emissor = "Excursoes.Api";
    public const string Audiencia = "Excursoes.Front";
    public const string OrigemDoFront = "http://localhost:5173";

    // PNG de 1x1 pixel válido
    public static readonly byte[] PngValido = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");

    private WebApplication? _app;
    private string _conexaoServidor = "";
    private string _banco = "";
    private string _pastaConteudo = "";
    private int _contador;

    public HttpClient Cliente { get; private set; } = null!;
    public string PastaUploads { get; private set; } = "";

    // Os testes de "limite de tentativas" usam um valor baixo; os outros, um bem alto
    protected virtual int LimiteLoginPorMinuto
    {
        get { return 10000; }
    }

    public async Task InitializeAsync()
    {
        string? conexao = Environment.GetEnvironmentVariable("EXCURSOES_TESTE_MYSQL");
        if (string.IsNullOrWhiteSpace(conexao))
            throw new InvalidOperationException(
                "Defina EXCURSOES_TESTE_MYSQL, ex.: server=localhost;user=root;password=SUA_SENHA");
        _conexaoServidor = conexao;

        _banco = "excursoes_teste_" + Guid.NewGuid().ToString("N").Substring(0, 10);
        await CriarBancoAsync();

        _pastaConteudo = Path.Combine(Path.GetTempPath(), _banco);
        Directory.CreateDirectory(_pastaConteudo);
        PastaUploads = Path.Combine(_pastaConteudo, "wwwroot", "uploads");

        WebApplicationOptions opcoes = new WebApplicationOptions
        {
            EnvironmentName = "Testes",
            ContentRootPath = _pastaConteudo,
            ApplicationName = typeof(Inicializacao).Assembly.GetName().Name   // acha os controllers da API
        };

        WebApplicationBuilder builder = WebApplication.CreateBuilder(opcoes);
        builder.Configuration.AddInMemoryCollection(Configuracao());
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
        builder.WebHost.UseUrls("http://127.0.0.1:0");

        Inicializacao.ConfigurarServicos(builder);
        _app = builder.Build();
        Inicializacao.ConfigurarPipeline(_app);
        await Inicializacao.PrepararAsync(_app);
        await _app.StartAsync();

        Cliente = new HttpClient();
        Cliente.BaseAddress = new Uri(EnderecoDaApi());
    }

    public async Task DisposeAsync()
    {
        Cliente.Dispose();
        if (_app != null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }

        using (MySqlConnection conexao = new MySqlConnection(_conexaoServidor))
        {
            await conexao.ExecuteAsync("DROP DATABASE IF EXISTS `" + _banco + "`");
        }

        if (Directory.Exists(_pastaConteudo))
            Directory.Delete(_pastaConteudo, true);
    }

    private Dictionary<string, string?> Configuracao()
    {
        MySqlConnectionStringBuilder conexao = new MySqlConnectionStringBuilder(_conexaoServidor);
        conexao.Database = _banco;

        Dictionary<string, string?> valores = new Dictionary<string, string?>();
        valores["ConnectionStrings:Padrao"] = conexao.ConnectionString;
        valores["Jwt:Chave"] = ChaveJwt;
        valores["Jwt:Emissor"] = Emissor;
        valores["Jwt:Audiencia"] = Audiencia;
        valores["Jwt:ExpiracaoMinutos"] = "60";
        valores["Cors:OrigensPermitidas:0"] = OrigemDoFront;
        valores["Seguranca:LimiteLoginPorMinuto"] = LimiteLoginPorMinuto.ToString();
        valores["Seguranca:LimiteGeralPorMinuto"] = "100000";
        valores["Seguranca:BcryptCusto"] = "4";   // rápido nos testes; em produção é 12
        valores["Uploads:TamanhoMaximoMb"] = "5";
        valores["Pagamentos:PermitirPagamentoSimulado"] = "true";
        valores["Admin:Email"] = EmailAdmin;
        valores["Admin:Senha"] = SenhaAdmin;
        valores["Admin:Nome"] = "Admin de Teste";
        return valores;
    }

    private async Task CriarBancoAsync()
    {
        string schema = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "schema.sql"));
        schema = schema.Replace("USE excursoes;", "USE `" + _banco + "`;");

        using (MySqlConnection conexao = new MySqlConnection(_conexaoServidor))
        {
            await conexao.OpenAsync();
            await conexao.ExecuteAsync("CREATE DATABASE `" + _banco + "` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci");
            await conexao.ExecuteAsync(schema);
        }
    }

    private string EnderecoDaApi()
    {
        IServer servidor = _app!.Services.GetRequiredService<IServer>();
        IServerAddressesFeature? enderecos = servidor.Features.Get<IServerAddressesFeature>();
        if (enderecos == null)
            throw new InvalidOperationException("Endereço da API de teste não encontrado.");

        foreach (string endereco in enderecos.Addresses)
        {
            return endereco;
        }
        throw new InvalidOperationException("Endereço da API de teste não encontrado.");
    }

    // ==================== Ajudantes para os testes ====================

    public string EmailUnico(string prefixo)
    {
        int numero = Interlocked.Increment(ref _contador);
        return prefixo + numero + "_" + Guid.NewGuid().ToString("N").Substring(0, 6) + "@teste.com";
    }

    public static HttpRequestMessage Pedido(HttpMethod metodo, string url, string? token)
    {
        HttpRequestMessage pedido = new HttpRequestMessage(metodo, url);
        if (token != null)
            pedido.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return pedido;
    }

    public async Task<string> LoginAsync(string email, string senha)
    {
        HttpResponseMessage resposta = await Cliente.PostAsJsonAsync("/api/auth/login", new { email = email, senha = senha });
        resposta.EnsureSuccessStatusCode();
        JsonElement corpo = await LerJson(resposta);
        return corpo.GetProperty("token").GetString()!;
    }

    public Task<string> TokenDoAdminAsync()
    {
        return LoginAsync(EmailAdmin, SenhaAdmin);
    }

    // Um segundo administrador (o "outro admin" dos ataques)
    public async Task<string> NovoAdminAsync()
    {
        string email = EmailUnico("admin");
        string senha = "SenhaDeOutroAdmin2026";
        using (IServiceScope escopo = _app!.Services.CreateScope())
        {
            IAuthService auth = escopo.ServiceProvider.GetRequiredService<IAuthService>();
            await auth.CriarAdminInicialAsync("Outro Admin", email, senha);
        }
        return await LoginAsync(email, senha);
    }

    public async Task<string> NovoPassageiroAsync()
    {
        HttpResponseMessage resposta = await Cliente.PostAsJsonAsync("/api/auth/cadastro",
            new { nome = "Passageiro Teste", email = EmailUnico("passageiro"), senha = "senha-segura-123" });
        resposta.EnsureSuccessStatusCode();
        JsonElement corpo = await LerJson(resposta);
        return corpo.GetProperty("token").GetString()!;
    }

    public static MultipartFormDataContent FormularioDeViagem(string valor = "1000.00", int vagas = 10, int maxParcelas = 10)
    {
        DateOnly saida = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30);
        MultipartFormDataContent form = new MultipartFormDataContent();
        form.Add(new StringContent("Viagem de teste"), "titulo");
        form.Add(new StringContent("Pirenópolis, GO"), "destino");
        form.Add(new StringContent("Descrição de teste"), "descricao");
        form.Add(new StringContent(saida.ToString("yyyy-MM-dd")), "dataSaida");
        form.Add(new StringContent(saida.AddDays(3).ToString("yyyy-MM-dd")), "dataRetorno");
        form.Add(new StringContent(valor), "valor");
        form.Add(new StringContent(vagas.ToString()), "vagas");
        form.Add(new StringContent(maxParcelas.ToString()), "maxParcelas");
        return form;
    }

    public static void AdicionarArquivo(MultipartFormDataContent form, byte[] bytes, string nome, string tipo)
    {
        ByteArrayContent arquivo = new ByteArrayContent(bytes);
        arquivo.Headers.ContentType = new MediaTypeHeaderValue(tipo);
        form.Add(arquivo, "imagens", nome);
    }

    public async Task<int> CriarViagemAsync(string tokenAdmin, string valor = "1000.00", int vagas = 10, int maxParcelas = 10)
    {
        HttpRequestMessage pedido = Pedido(HttpMethod.Post, "/api/viagens", tokenAdmin);
        MultipartFormDataContent form = FormularioDeViagem(valor, vagas, maxParcelas);
        AdicionarArquivo(form, PngValido, "capa.png", "image/png");
        pedido.Content = form;

        HttpResponseMessage resposta = await Cliente.SendAsync(pedido);
        if (!resposta.IsSuccessStatusCode)
            throw new InvalidOperationException("Criar viagem falhou: " + await resposta.Content.ReadAsStringAsync());
        JsonElement corpo = await LerJson(resposta);
        return corpo.GetProperty("id").GetInt32();
    }

    public async Task<HttpResponseMessage> ComprarAsync(string tokenPassageiro, int viagemId, string metodo, int parcelas)
    {
        HttpRequestMessage pedido = Pedido(HttpMethod.Post, "/api/viagens/" + viagemId + "/comprar", tokenPassageiro);
        pedido.Content = JsonContent.Create(new { metodo = metodo, numParcelas = parcelas });
        return await Cliente.SendAsync(pedido);
    }

    public static async Task<JsonElement> LerJson(HttpResponseMessage resposta)
    {
        string texto = await resposta.Content.ReadAsStringAsync();
        using (JsonDocument documento = JsonDocument.Parse(texto))
        {
            return documento.RootElement.Clone();
        }
    }

    public static async Task<string> MensagemDeErro(HttpResponseMessage resposta)
    {
        JsonElement corpo = await LerJson(resposta);
        return corpo.GetProperty("erro").GetString()!;
    }
}

// Mesma API, mas com limite de 3 tentativas de login por minuto
public class ApiComLimiteBaixo : ApiDeTeste
{
    protected override int LimiteLoginPorMinuto
    {
        get { return 3; }
    }
}
