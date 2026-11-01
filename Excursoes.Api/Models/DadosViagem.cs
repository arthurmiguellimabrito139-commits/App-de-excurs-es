namespace Excursoes.Api.Models;

// Os campos de uma viagem juntos num objeto só (serve para criar e editar)
public class DadosViagem
{
    public string Titulo { get; }
    public string Destino { get; }
    public string Descricao { get; }
    public DateOnly DataSaida { get; }
    public DateOnly DataRetorno { get; }
    public decimal Valor { get; }
    public int Vagas { get; }
    public int MaxParcelas { get; }

    public DadosViagem(string titulo, string destino, string descricao, DateOnly dataSaida,
                       DateOnly dataRetorno, decimal valor, int vagas, int maxParcelas)
    {
        Titulo = titulo;
        Destino = destino;
        Descricao = descricao;
        DataSaida = dataSaida;
        DataRetorno = dataRetorno;
        Valor = valor;
        Vagas = vagas;
        MaxParcelas = maxParcelas;
    }
}
