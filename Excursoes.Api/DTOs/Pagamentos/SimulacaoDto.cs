using Excursoes.Api.Strategies.Pagamento;

namespace Excursoes.Api.DTOs.Pagamentos;

public record SimulacaoDto(
    string Metodo,
    int NumParcelas,
    int MaxParcelas,
    decimal Total,
    IReadOnlyList<ParcelaCalculada> Parcelas);