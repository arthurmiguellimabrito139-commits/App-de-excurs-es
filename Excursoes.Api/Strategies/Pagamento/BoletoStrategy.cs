namespace Excursoes.Api.Strategies.Pagamento;

public class BoletoStrategy : PagamentoStrategyBase
{
    public override string Metodo { get { return "boleto"; } }
    public override string Nome { get { return "Boleto"; } }
    public override string Descricao { get { return "Parcelado sem juros, um boleto por mês"; } }

    protected override decimal CalcularTotal(decimal valor, int numParcelas)
    {
        return valor;
    }
}
