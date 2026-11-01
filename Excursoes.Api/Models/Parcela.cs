using Excursoes.Api.Excecoes;

namespace Excursoes.Api.Models;

public class Parcela : Entidade
{
    public int CompraId { get; private set; }
    public int Numero { get; private set; }
    public decimal Valor { get; private set; }
    public DateOnly Vencimento { get; private set; }
    public bool Paga { get; private set; }
    public DateTime? PagoEm { get; private set; }   // null enquanto não for paga

    private Parcela()
    {
    }

    internal Parcela(int numero, decimal valor, DateOnly vencimento)
    {
        if (numero < 1)
            throw new RegraNegocioException("Número da parcela inválido.");
        if (valor <= 0)
            throw new RegraNegocioException("Valor da parcela deve ser maior que zero.");

        Numero = numero;
        Valor = valor;
        Vencimento = vencimento;
        Paga = false;
    }

    internal void MarcarComoPaga()
    {
        if (Paga)
            throw new RegraNegocioException("Esta parcela já foi paga.");

        Paga = true;
        PagoEm = DateTime.UtcNow;
    }
}
