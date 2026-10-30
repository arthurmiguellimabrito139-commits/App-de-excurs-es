using Excursoes.Api.Models;

namespace Excursoes.Api.DTOs.Pagamentos;

public record ParcelaDto(int Id, int Numero, decimal Valor, DateOnly Vencimento, string Status, DateTime? PagoEm)
{
    public static ParcelaDto De(Parcela parcela)
    {
        string status = "aberta";
        if (parcela.Paga)
            status = "paga";

        return new ParcelaDto(parcela.Id, parcela.Numero, parcela.Valor, parcela.Vencimento, status, parcela.PagoEm);
    }
}