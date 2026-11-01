using Excursoes.Api.Excecoes;

namespace Excursoes.Api.Strategies.Pagamento;

// Escolhe a strategy pelo nome do método ("pix", "cartao", "boleto")
public class PagamentoContext
{
    private readonly Dictionary<string, IPagamentoStrategy> _estrategias = new Dictionary<string, IPagamentoStrategy>();
    private readonly List<IPagamentoStrategy> _emOrdem = new List<IPagamentoStrategy>();

    // O ASP.NET entrega aqui todas as strategies registradas no Program
    public PagamentoContext(IEnumerable<IPagamentoStrategy> estrategias)
    {
        foreach (IPagamentoStrategy estrategia in estrategias)
        {
            _estrategias[estrategia.Metodo] = estrategia;
            _emOrdem.Add(estrategia);
        }
    }

    public IPagamentoStrategy Obter(string? metodo)
    {
        if (metodo != null && _estrategias.TryGetValue(metodo, out IPagamentoStrategy? estrategia))
            return estrategia;

        throw new RegraNegocioException("Forma de pagamento inválida.");
    }

    public IReadOnlyList<IPagamentoStrategy> Todas()
    {
        return _emOrdem;
    }
}
