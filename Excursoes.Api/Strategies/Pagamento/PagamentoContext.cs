using Excursoes.Api.Excecoes;

namespace Excursoes.Api.Strategies.Pagamento;

public class PagamentoContext
{
    private readonly Dictionary<string, IPagamentoStrategy> _estrategias = new();

    // O ASP.NET entrega aqui todas as strategies registradas no Program.cs
    public PagamentoContext(IEnumerable<IPagamentoStrategy> estrategias)
    {
        foreach (IPagamentoStrategy estrategia in estrategias)
        {
            _estrategias[estrategia.Metodo] = estrategia;
        }
    }

    public IPagamentoStrategy Obter(string metodo)
    {
        if (_estrategias.TryGetValue(metodo, out IPagamentoStrategy? estrategia))
            return estrategia;

        throw new RegraNegocioException("Forma de pagamento inválida.");
    }

    public IEnumerable<IPagamentoStrategy> Todas()
    {
        return _estrategias.Values;
    }
}