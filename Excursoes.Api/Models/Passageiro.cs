using Excursoes.Api.Excecoes;
using Excursoes.Api.Strategies.Pagamento;

namespace Excursoes.Api.Models;

public class Passageiro : Usuario
{
    private readonly List<Compra> _compras = new List<Compra>();

    public IReadOnlyCollection<Compra> Compras
    {
        get { return _compras; }
    }

    public override string Papel
    {
        get { return "Passageiro"; }
    }

    private Passageiro()
    {
    }

    public Passageiro(string nome, string email, string senhaHash)
        : base(nome, email, senhaHash)
    {
    }

    public Compra Comprar(Viagem viagem, IPagamentoStrategy strategy, int numParcelas)
    {
        // regra do passageiro
        if (JaComprou(viagem))
            throw new RegraNegocioException("Você já comprou esta viagem.");

        // a viagem confere a vaga e usa a strategy para gerar as parcelas
        Compra compra = viagem.Vender(this, strategy, numParcelas);
        _compras.Add(compra);
        return compra;
    }

    public bool JaComprou(Viagem viagem)
    {
        foreach (Compra compra in _compras)
        {
            if (compra.ViagemId == viagem.Id && compra.Ativa)
                return true;
        }
        return false;
    }

    internal void CarregarCompras(IEnumerable<Compra> compras)
    {
        _compras.Clear();
        _compras.AddRange(compras);
    }
}
