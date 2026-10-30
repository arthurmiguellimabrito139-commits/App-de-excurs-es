namespace Excursoes.Api.Models;

public record DadosViagem(
    string Titulo,
    string Destino,
    string Descricao,
    DateOnly DataSaida,
    DateOnly DataRetorno,
    decimal Valor,
    int Vagas,
    int MaxParcelas);