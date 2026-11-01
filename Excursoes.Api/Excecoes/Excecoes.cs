namespace Excursoes.Api.Excecoes;

// Cada exceção vira um status HTTP no ErroMiddleware.

// Regra de negócio quebrada (ex.: "a volta não pode ser antes da ida") -> 400
public class RegraNegocioException : Exception
{
    public RegraNegocioException(string mensagem) : base(mensagem)
    {
    }
}

// Recurso não existe OU não pertence a quem pediu -> 404
// (responder 404 em vez de 403 não confirma que o recurso existe)
public class NaoEncontradoException : Exception
{
    public NaoEncontradoException(string mensagem) : base(mensagem)
    {
    }
}

// Login inválido -> 401
public class NaoAutorizadoException : Exception
{
    public NaoAutorizadoException(string mensagem) : base(mensagem)
    {
    }
}

// Alguém alterou o mesmo dado ao mesmo tempo (ex.: última vaga) -> 409
public class ConflitoException : Exception
{
    public ConflitoException(string mensagem) : base(mensagem)
    {
    }
}
