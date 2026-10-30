using Excursoes.Api.Excecoes;
using Excursoes.Api.Models;

namespace Excursoes.Api.Strategies.Pagamento;

public abstract class PagamentoStrategyBase : IPagamentoStrategy
{
    public abstract string Metodo { get; }
    public abstract string Nome { get; }
    public abstract string Descricao { get; }

    // Por padrão, o limite é o que o admin definiu na viagem. As filhas podem mudar.
    public virtual int MaxParcelas(Viagem viagem)
    {
        return viagem.MaxParcelas;
    }

    // O único passo que muda de uma forma de pagamento para outra
    protected abstract decimal CalcularTotal(decimal valor, int numParcelas);

    public IReadOnlyList<ParcelaCalculada> Calcular(Viagem viagem, int numParcelas)
    {
        int max = MaxParcelas(viagem);
        if (numParcelas < 1 || numParcelas > max)
        {
            if (max == 1)
                throw new RegraNegocioException(Nome + " só aceita pagamento à vista.");
            throw new RegraNegocioException(Nome + ": escolha entre 1 e " + max + " parcelas.");
        }

        decimal total = Math.Round(CalcularTotal(viagem.Valor, numParcelas), 2);

        // Arredonda para baixo; a última parcela fica com a diferença dos centavos
        decimal valorParcela = Math.Floor(total / numParcelas * 100) / 100;
        decimal valorUltima = total - valorParcela * (numParcelas - 1);

        DateOnly hoje = DateOnly.FromDateTime(DateTime.Today);
        List<ParcelaCalculada> parcelas = new List<ParcelaCalculada>();

        for (int numero = 1; numero <= numParcelas; numero++)
        {
            decimal valor = valorParcela;
            if (numero == numParcelas)
                valor = valorUltima;

            DateOnly vencimento = hoje.AddMonths(numero - 1);
            parcelas.Add(new ParcelaCalculada(numero, valor, vencimento));
        }

        return parcelas;
    }
}