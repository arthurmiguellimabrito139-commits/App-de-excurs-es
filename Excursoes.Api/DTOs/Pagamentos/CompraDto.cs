using Excursoes.Api.Models;

namespace Excursoes.Api.DTOs.Pagamentos;

public record CompraDto(
    int Id,
    string Metodo,
    int NumParcelas,
    decimal ValorTotal,
    string Status,
    DateTime CriadoEm,
    ViagemResumoDto Viagem,
    List<ParcelaDto> Parcelas)
{
    public static CompraDto De(Compra compra)
    {
        List<ParcelaDto> parcelas = new List<ParcelaDto>();
        foreach (Parcela parcela in compra.Parcelas)
        {
            parcelas.Add(ParcelaDto.De(parcela));
        }

        return new CompraDto(
            compra.Id,
            compra.Metodo,
            compra.Parcelas.Count,
            compra.ValorTotal,
            compra.Status.ToString().ToLower(),   // Pendente → "pendente"
            compra.CriadoEm,
            ViagemResumoDto.De(compra.Viagem),
            parcelas);
    }
}