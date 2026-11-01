namespace Excursoes.Api.Services;

// O banco guarda só o caminho da imagem (/uploads/abc.webp).
// Na resposta, a API devolve o endereço completo (http://.../uploads/abc.webp),
// porque o front roda em outro endereço.
public interface IEnderecoPublico
{
    string Completar(string caminho);
}

public class EnderecoPublico : IEnderecoPublico
{
    private readonly IHttpContextAccessor _acessor;

    public EnderecoPublico(IHttpContextAccessor acessor)
    {
        _acessor = acessor;
    }

    public string Completar(string caminho)
    {
        HttpContext? contexto = _acessor.HttpContext;
        if (contexto == null)
            return caminho;

        HttpRequest requisicao = contexto.Request;
        return requisicao.Scheme + "://" + requisicao.Host.Value + requisicao.PathBase.Value + caminho;
    }
}
