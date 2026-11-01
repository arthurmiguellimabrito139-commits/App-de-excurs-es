using Excursoes.Api.Configuracao;

// Cada etapa está explicada em Configuracao/Inicializacao.cs e Configuracao/Seguranca.cs
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
Inicializacao.ConfigurarServicos(builder);

WebApplication app = builder.Build();
Inicializacao.ConfigurarPipeline(app);
await Inicializacao.PrepararAsync(app);

app.Run();
