using Excursoes.Api.Models;

namespace Excursoes.Api.Strategies.Pagamento;

public class PixStrategy : PagamentoStrategyBase
{
    public override string Metodo { get { return "pix"; } }
    public override string Nome { get { return "Pix"; } }
    public override string Descricao { get { return "À vista com 5% de desconto"; } }

    public override int MaxParcelas(Viagem viagem)
    {
        return 1;
    }

    protected override decimal CalcularTotal(decimal valor, int numParcelas)
    {
        return valor * 0.95m;
    }
}
