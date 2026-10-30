using Excursoes.Api.Models;

namespace Excursoes.Api.DTOs.Viagens;

public record ViagemDto(
    int Id,
    int AdminId,
    string Titulo,
    string Destino,
    string Descricao,
    DateOnly DataSaida,
    DateOnly DataRetorno,
    decimal Valor,
    int Vagas,
    int VagasDisponiveis,
    int MaxParcelas,
    List<ImagemDto> Imagens)
{
    public static ViagemDto De(Viagem viagem)
    {
        List<ImagemDto> imagens = new List<ImagemDto>();
        foreach (ViagemImagem imagem in viagem.Imagens)
        {
            imagens.Add(new ImagemDto(imagem.Id, imagem.Url));
        }

        return new ViagemDto(
            viagem.Id, viagem.AdminId, viagem.Titulo, viagem.Destino, viagem.Descricao,
            viagem.DataSaida, viagem.DataRetorno, viagem.Valor,
            viagem.Vagas, viagem.VagasDisponiveis, viagem.MaxParcelas, imagens);
    }
}