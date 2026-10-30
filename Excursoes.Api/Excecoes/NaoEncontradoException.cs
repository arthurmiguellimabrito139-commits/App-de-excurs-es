namespace Excursoes.Api.Excecoes;

public class NaoEncontradoException : Exception
{
    public NaoEncontradoException(string mensagem) : base(mensagem) { }
}