using Excursoes.Api.Excecoes;

namespace Excursoes.Api.Middlewares;

// Fica em volta de todas as requisições.
// Transforma exceções em respostas { "erro": "..." } com o status certo
// e NUNCA mostra detalhes técnicos (stack trace, SQL, caminhos) para quem chamou.
public class ErroMiddleware
{
    private readonly RequestDelegate _proximo;
    private readonly ILogger<ErroMiddleware> _log;

    public ErroMiddleware(RequestDelegate proximo, ILogger<ErroMiddleware> log)
    {
        _proximo = proximo;
        _log = log;
    }

    public async Task Invoke(HttpContext contexto)
    {
        try
        {
            await _proximo(contexto);
        }
        catch (RegraNegocioException erro)
        {
            await Responder(contexto, StatusCodes.Status400BadRequest, erro.Message);
        }
        catch (NaoEncontradoException erro)
        {
            await Responder(contexto, StatusCodes.Status404NotFound, erro.Message);
        }
        catch (NaoAutorizadoException erro)
        {
            await Responder(contexto, StatusCodes.Status401Unauthorized, erro.Message);
        }
        catch (ConflitoException erro)
        {
            await Responder(contexto, StatusCodes.Status409Conflict, erro.Message);
        }
        catch (BadHttpRequestException erro)
        {
            // Ex.: upload maior que o permitido (413)
            string mensagem = "Requisição inválida.";
            if (erro.StatusCode == StatusCodes.Status413PayloadTooLarge)
                mensagem = "Arquivos grandes demais.";
            await Responder(contexto, erro.StatusCode, mensagem);
        }
        catch (OperationCanceledException)
        {
            // O cliente desistiu da requisição (fechou a aba): nada a responder
        }
        catch (Exception erro)
        {
            // O detalhe vai SÓ para o log do servidor
            _log.LogError(erro, "Erro não tratado em {Metodo} {Caminho}", contexto.Request.Method, contexto.Request.Path);
            await Responder(contexto, StatusCodes.Status500InternalServerError, "Algo deu errado. Tente novamente.");
        }
    }

    private static async Task Responder(HttpContext contexto, int status, string mensagem)
    {
        if (contexto.Response.HasStarted)
            return;

        contexto.Response.Clear();
        contexto.Response.StatusCode = status;
        await contexto.Response.WriteAsJsonAsync(new RespostaErro(mensagem));
    }
}

public class RespostaErro
{
    public string Erro { get; }

    public RespostaErro(string erro)
    {
        Erro = erro;
    }
}
