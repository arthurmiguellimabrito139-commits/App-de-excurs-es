namespace Excursoes.Api.Middlewares;

// Cabeçalhos de segurança em TODAS as respostas
public class CabecalhosSegurancaMiddleware
{
    private readonly RequestDelegate _proximo;

    public CabecalhosSegurancaMiddleware(RequestDelegate proximo)
    {
        _proximo = proximo;
    }

    public Task Invoke(HttpContext contexto)
    {
        IHeaderDictionary cabecalhos = contexto.Response.Headers;

        // O navegador não tenta "adivinhar" o tipo do arquivo (uma imagem nunca vira script)
        cabecalhos["X-Content-Type-Options"] = "nosniff";
        // A API não pode ser exibida dentro de iframe de outro site (clickjacking)
        cabecalhos["X-Frame-Options"] = "DENY";
        // Não vaza o endereço da API para outros sites
        cabecalhos["Referrer-Policy"] = "no-referrer";
        // A API só devolve JSON e imagens: nada de scripts, estilos ou frames
        cabecalhos["Content-Security-Policy"] = "default-src 'none'; img-src 'self'; frame-ancestors 'none'";
        // Desliga recursos do navegador que a API nunca usa
        cabecalhos["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

        return _proximo(contexto);
    }
}
