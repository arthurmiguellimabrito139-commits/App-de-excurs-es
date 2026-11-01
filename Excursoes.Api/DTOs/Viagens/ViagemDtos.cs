using System.ComponentModel.DataAnnotations;
using Excursoes.Api.Models;
using Excursoes.Api.Services;

namespace Excursoes.Api.DTOs.Viagens;

// ---------- Entrada (formulário multipart, por causa das imagens) ----------

public class ViagemFormDto
{
    [Required(ErrorMessage = "Informe o título.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O título deve ter entre 3 e 150 caracteres.")]
    public string Titulo { get; set; } = "";

    [Required(ErrorMessage = "Informe o destino.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "O destino deve ter entre 2 e 150 caracteres.")]
    public string Destino { get; set; } = "";

    [StringLength(4000, ErrorMessage = "Descrição muito longa (máximo 4000 caracteres).")]
    public string? Descricao { get; set; }

    [Required(ErrorMessage = "Informe a data de ida.")]
    public DateOnly? DataSaida { get; set; }

    [Required(ErrorMessage = "Informe a data de volta.")]
    public DateOnly? DataRetorno { get; set; }

    [Range(typeof(decimal), "0.01", "1000000", ErrorMessage = "Valor inválido.")]
    public decimal Valor { get; set; }

    [Range(1, 500, ErrorMessage = "Vagas entre 1 e 500.")]
    public int Vagas { get; set; }

    [Range(1, 12, ErrorMessage = "Parcelas máximas entre 1 e 12.")]
    public int MaxParcelas { get; set; }

    public List<IFormFile> Imagens { get; set; } = new List<IFormFile>();      // fotos novas
    public List<int> ImagensRemover { get; set; } = new List<int>();            // ids das fotos a tirar (editar)

    public DadosViagem ParaDados()
    {
        string descricao = "";
        if (Descricao != null)
            descricao = Descricao;

        return new DadosViagem(Titulo, Destino, descricao,
            DataSaida.GetValueOrDefault(), DataRetorno.GetValueOrDefault(),
            Valor, Vagas, MaxParcelas);
    }
}

// ---------- Saída ----------

public class ImagemDto
{
    public int Id { get; set; }
    public string Url { get; set; } = "";
}

public class ViagemDto
{
    public int Id { get; set; }
    public int AdminId { get; set; }
    public string Titulo { get; set; } = "";
    public string Destino { get; set; } = "";
    public string Descricao { get; set; } = "";
    public DateOnly DataSaida { get; set; }
    public DateOnly DataRetorno { get; set; }
    public decimal Valor { get; set; }
    public int Vagas { get; set; }
    public int VagasDisponiveis { get; set; }
    public int MaxParcelas { get; set; }
    public List<ImagemDto> Imagens { get; set; } = new List<ImagemDto>();

    public static ViagemDto De(Viagem viagem, IEnderecoPublico endereco)
    {
        ViagemDto dto = new ViagemDto();
        dto.Id = viagem.Id;
        dto.AdminId = viagem.AdminId;
        dto.Titulo = viagem.Titulo;
        dto.Destino = viagem.Destino;
        dto.Descricao = viagem.Descricao;
        dto.DataSaida = viagem.DataSaida;
        dto.DataRetorno = viagem.DataRetorno;
        dto.Valor = viagem.Valor;
        dto.Vagas = viagem.Vagas;
        dto.VagasDisponiveis = viagem.VagasDisponiveis;
        dto.MaxParcelas = viagem.MaxParcelas;

        foreach (ViagemImagem imagem in viagem.Imagens)
        {
            ImagemDto imagemDto = new ImagemDto();
            imagemDto.Id = imagem.Id;
            imagemDto.Url = endereco.Completar(imagem.Url);
            dto.Imagens.Add(imagemDto);
        }
        return dto;
    }
}
