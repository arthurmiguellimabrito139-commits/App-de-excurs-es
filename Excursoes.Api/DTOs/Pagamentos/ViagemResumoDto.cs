using Excursoes.Api.Models;

namespace Excursoes.Api.DTOs.Pagamentos;

public record ViagemResumoDto(int Id, string Titulo, string Destino, DateOnly DataSaida, string? Imagem)
{
    public static ViagemResumoDto De(Viagem viagem)
    {
        string? capa = null;
        foreach (ViagemImagem imagem in viagem.Imagens)
        {
            capa = imagem.Url;   // pega a primeira e para
            break;
        }

        return new ViagemResumoDto(viagem.Id, viagem.Titulo, viagem.Destino, viagem.DataSaida, capa);
    }
}