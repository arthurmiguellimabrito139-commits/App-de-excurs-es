namespace Excursoes.Api.Strategies.Pagamento;

// Resultado do cálculo de uma strategy: ainda NÃO é uma Parcela do banco
public class ParcelaCalculada
{
    public int Numero { get; }
    public decimal Valor { get; }
    public DateOnly Vencimento { get; }

    public ParcelaCalculada(int numero, decimal valor, DateOnly vencimento)
    {
        Numero = numero;
        Valor = valor;
        Vencimento = vencimento;
    }
}
