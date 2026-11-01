using Excursoes.Api.Models;

namespace Excursoes.Api.Strategies.Pagamento;

// Contrato que toda forma de pagamento precisa cumprir (padrão Strategy)
public interface IPagamentoStrategy
{
    string Metodo { get; }      // "pix", "cartao", "boleto"
    string Nome { get; }
    string Descricao { get; }

    int MaxParcelas(Viagem viagem);
    IReadOnlyList<ParcelaCalculada> Calcular(Viagem viagem, int numParcelas);
}
