using Excursoes.Api.Models;

namespace Excursoes.Api.Strategies.Pagamento;

public interface IPagamentoStrategy
{
    string Metodo { get; }      
    string Nome { get; }
    string Descricao { get; }

    int MaxParcelas(Viagem viagem);
    IReadOnlyList<ParcelaCalculada> Calcular(Viagem viagem, int numParcelas);
}