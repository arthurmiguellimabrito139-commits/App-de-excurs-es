namespace Excursoes.Api.Excecoes;

public class RegraNegocioException : Exception
{
    public RegraNegocioException(string mensagem) : base(mensagem) { }
}