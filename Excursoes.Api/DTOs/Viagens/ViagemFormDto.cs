using System.ComponentModel.DataAnnotations;
using Excursoes.Api.Models;

namespace Excursoes.Api.DTOs.Viagens;

public class ViagemFormDto
{
    [Required, StringLength(150, MinimumLength = 3)]
    public string Titulo { get; set; } = "";

    [Required, StringLength(150, MinimumLength = 2)]
    public string Destino { get; set; } = "";

    [StringLength(4000)]
    public string Descricao { get; set; } = "";

    [Required]
    public DateOnly DataSaida { get; set; }

    [Required]
    public DateOnly DataRetorno { get; set; }

    [Range(1, 1_000_000, ErrorMessage = "Valor inválido.")]
    public decimal Valor { get; set; }

    [Range(1, 500, ErrorMessage = "Vagas entre 1 e 500.")]
    public int Vagas { get; set; }

    [Range(1, 12, ErrorMessage = "Parcelas máximas entre 1 e 12.")]
    public int MaxParcelas { get; set; }

    public List<IFormFile> Imagens { get; set; } = new();        // fotos novas
    public List<int> ImagensRemover { get; set; } = new();       // ids das fotos a tirar (editar)

    // Converte para o objeto que o model entende
    public DadosViagem ParaDados()
    {
        return new DadosViagem(Titulo, Destino, Descricao, DataSaida, DataRetorno, Valor, Vagas, MaxParcelas);
    }
}