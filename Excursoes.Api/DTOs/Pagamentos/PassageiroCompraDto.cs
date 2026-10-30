using Excursoes.Api.Models;

namespace Excursoes.Api.DTOs.Pagamentos;

public record PassageiroCompraDto(
    int CompraId,
    PassageiroResumoDto Passageiro,
    string Metodo,
    int NumParcelas,
    int ParcelasPagas,
    decimal ValorTotal,
    decimal ValorPago,
    string Status,
    DateTime CriadoEm)
{
    public static PassageiroCompraDto De(Compra compra)
    {
        PassageiroResumoDto passageiro = new PassageiroResumoDto(
            compra.Passageiro.Id, compra.Passageiro.Nome, compra.Passageiro.Email);

        return new PassageiroCompraDto(
            compra.Id,
            passageiro,
            compra.Metodo,
            compra.Parcelas.Count,
            compra.ParcelasPagas(),
            compra.ValorTotal,
            compra.ValorPago(),
            compra.Status.ToString().ToLower(),
            compra.CriadoEm);
    }
}