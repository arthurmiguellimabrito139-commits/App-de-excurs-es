using Excursoes.Api.Models;

namespace Excursoes.Api.Strategies.Pagamento;

public class CartaoStrategy : PagamentoStrategyBase
{
    private const decimal JurosAoMes = 0.025m;   // 2,5%
    private const int ParcelasSemJuros = 3;

    public override string Metodo { get { return "cartao"; } }
    public override string Nome { get { return "Cartão de crédito"; } }
    public override string Descricao { get { return "Até 3x sem juros; acima disso, 2,5% ao mês"; } }

    public override int MaxParcelas(Viagem viagem)
    {
        return Math.Min(12, viagem.MaxParcelas);
    }

    protected override decimal CalcularTotal(decimal valor, int numParcelas)
    {
        if (numParcelas <= ParcelasSemJuros)
            return valor;

        // Juros compostos: valor x 1,025 elevado ao número de parcelas.
        // Feito com "for" porque Math.Pow só trabalha com double, que erra centavos.
        decimal total = valor;
        for (int i = 0; i < numParcelas; i++)
        {
            total = total * (1 + JurosAoMes);
        }
        return total;
    }
}
