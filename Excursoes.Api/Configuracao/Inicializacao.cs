using System.Globalization;
using Dapper;
using Excursoes.Api.Data;
using Excursoes.Api.Middlewares;
using Excursoes.Api.Repositories;
using Excursoes.Api.Services;
using Excursoes.Api.Strategies.Pagamento;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using MySqlConnector;

namespace Excursoes.Api.Configuracao;

// Monta a API. O Program.cs só chama estes três métodos,
// e os testes de ataque usam exatamente os mesmos.
public static class Inicializacao
{
    // ==================== 1. Serviços (injeção de dependência) ====================

    public static void ConfigurarServicos(WebApplicationBuilder builder)
    {
        // O front manda "1890.50": o ponto é sempre o separador decimal,
        // mesmo num Windows em português
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

        IServiceCollection servicos = builder.Services;
        IConfiguration configuracao = builder.Configuration;

        JwtOpcoes jwt = JwtOpcoes.Ler(configuracao);   // falha ao iniciar se a chave for fraca
        servicos.AddSingleton(jwt);

        // Banco
        servicos.AddSingleton<ConexaoFactory>();

        // Repositórios (SQL)
        servicos.AddScoped<IUsuarioRepository, UsuarioRepository>();
        servicos.AddScoped<IViagemRepository, ViagemRepository>();
        servicos.AddScoped<ICompraRepository, CompraRepository>();

        // Services (regras + coordenação)
        servicos.AddHttpContextAccessor();
        servicos.AddSingleton<IEnderecoPublico, EnderecoPublico>();
        servicos.AddSingleton<IImagemStorage, ImagemLocalStorage>();
        servicos.AddSingleton<ITokenService, TokenService>();
        servicos.AddScoped<IAuthService, AuthService>();
        servicos.AddScoped<IViagemService, ViagemService>();
        servicos.AddScoped<ICompraService, CompraService>();

        // Strategy: uma linha por forma de pagamento. Para criar uma nova,
        // é só escrever a classe e registrar aqui; nada mais muda.
        servicos.AddSingleton<IPagamentoStrategy, PixStrategy>();
        servicos.AddSingleton<IPagamentoStrategy, CartaoStrategy>();
        servicos.AddSingleton<IPagamentoStrategy, BoletoStrategy>();
        servicos.AddSingleton<PagamentoContext>();

        // Segurança
        Seguranca.AdicionarAutenticacao(servicos, jwt);
        Seguranca.AdicionarCors(servicos, configuracao);
        Seguranca.AdicionarLimites(servicos, configuracao);
        servicos.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(Seguranca.ConfigurarFormularios);
        builder.WebHost.ConfigureKestrel(Seguranca.ConfigurarKestrel);

        if (builder.Environment.IsProduction())
            servicos.Configure<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions>(Seguranca.ConfigurarProxy);

        servicos.AddControllers(Seguranca.ConfigurarMvc)
                .ConfigureApiBehaviorOptions(Seguranca.ConfigurarRespostaDeValidacao);
    }

    // ==================== 2. Pipeline (a ordem importa) ====================

    public static void ConfigurarPipeline(WebApplication app)
    {
        if (app.Environment.IsProduction())
        {
            app.UseForwardedHeaders();
            app.UseHsts();                 // navegador passa a exigir https
            app.UseHttpsRedirection();
        }

        app.UseMiddleware<CabecalhosSegurancaMiddleware>();
        app.UseMiddleware<ErroMiddleware>();   // erro sem detalhe para o usuário

        app.UseRouting();
        app.UseCors(Seguranca.PoliticaCors);
        app.UseRateLimiter();
        app.UseStaticFiles(OpcoesDasImagens(app.Environment));

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
    }

    // Só a pasta de uploads é pública, e só com tipos de imagem
    private static StaticFileOptions OpcoesDasImagens(IWebHostEnvironment ambiente)
    {
        string pasta = ImagemLocalStorage.PastaUploads(ambiente);
        Directory.CreateDirectory(pasta);

        FileExtensionContentTypeProvider tipos = new FileExtensionContentTypeProvider();
        tipos.Mappings.Clear();
        tipos.Mappings[".jpg"] = "image/jpeg";
        tipos.Mappings[".png"] = "image/png";
        tipos.Mappings[".webp"] = "image/webp";

        StaticFileOptions opcoes = new StaticFileOptions();
        opcoes.FileProvider = new PhysicalFileProvider(pasta);
        opcoes.RequestPath = "/uploads";
        opcoes.ContentTypeProvider = tipos;
        opcoes.ServeUnknownFileTypes = false;
        opcoes.OnPrepareResponse = PrepararImagem;
        return opcoes;
    }

    private static void PrepararImagem(StaticFileResponseContext contexto)
    {
        IHeaderDictionary cabecalhos = contexto.Context.Response.Headers;
        cabecalhos["Cache-Control"] = "public, max-age=604800";
        // As imagens são exibidas pelo front, que fica em outro endereço
        cabecalhos["Cross-Origin-Resource-Policy"] = "cross-origin";
    }

    // ==================== 3. Antes de abrir: banco e admin inicial ====================

    public static async Task PrepararAsync(WebApplication app)
    {
        ILogger log = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Inicializacao");

        ConexaoFactory fabrica = app.Services.GetRequiredService<ConexaoFactory>();
        using (MySqlConnection conexao = fabrica.Criar())
        {
            await conexao.OpenAsync();
        }
        log.LogInformation("Conectado ao MySQL.");

        await CriarAdminInicialAsync(app, log);
    }

    // O cadastro público só cria Passageiro. O admin vem da configuração (user-secrets / variáveis de ambiente).
    private static async Task CriarAdminInicialAsync(WebApplication app, ILogger log)
    {
        IConfiguration configuracao = app.Configuration;
        string? email = configuracao["Admin:Email"];
        string? senha = configuracao["Admin:Senha"];
        string? nome = configuracao["Admin:Nome"];
        if (string.IsNullOrWhiteSpace(nome))
            nome = "Administrador";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
        {
            log.LogWarning("Admin:Email e Admin:Senha não configurados: nenhum administrador inicial foi criado.");
            return;
        }

        using (IServiceScope escopo = app.Services.CreateScope())
        {
            IUsuarioRepository usuarios = escopo.ServiceProvider.GetRequiredService<IUsuarioRepository>();
            if (await usuarios.EmailExisteAsync(email))
                return;

            IAuthService auth = escopo.ServiceProvider.GetRequiredService<IAuthService>();
            await auth.CriarAdminInicialAsync(nome, email, senha);
            log.LogInformation("Administrador inicial criado: {Email}", email);
        }
    }
}
