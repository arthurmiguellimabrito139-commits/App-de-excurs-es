using System.ComponentModel.DataAnnotations;
using Excursoes.Api.Models;
using Excursoes.Api.Services;
using Excursoes.Api.Strategies.Pagamento;

namespace Excursoes.Api.DTOs.Pagamentos;

// ---------- Entrada ----------

// Não tem campo de valor: o valor SEMPRE vem do banco
public class CompraRequestDto
{
    [Required(ErrorMessage = "Escolha a forma de pagamento.")]
    [StringLength(20, ErrorMessage = "Forma de pagamento inválida.")]
    public string Metodo { get; set; } = "";

    [Range(1, 12, ErrorMessage = "Número de parcelas inválido.")]
    public int NumParcelas { get; set; }
}

// ---------- Saída ----------

public class MetodoPagamentoDto
{
    public string Metodo { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Descricao { get; set; } = "";

    public static MetodoPagamentoDto De(IPagamentoStrategy strategy)
    {
        MetodoPagamentoDto dto = new MetodoPagamentoDto();
        dto.Metodo = strategy.Metodo;
        dto.Nome = strategy.Nome;
        dto.Descricao = strategy.Descricao;
        return dto;
    }
}

public class SimulacaoDto
{
    public string Metodo { get; set; } = "";
    public int NumParcelas { get; set; }
    public int MaxParcelas { get; set; }
    public decimal Total { get; set; }
    public IReadOnlyList<ParcelaCalculada> Parcelas { get; set; } = new List<ParcelaCalculada>();
}

public class ParcelaDto
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public decimal Valor { get; set; }
    public DateOnly Vencimento { get; set; }
    public string Status { get; set; } = "";     // "aberta" ou "paga"
    public DateTime? PagoEm { get; set; }

    public static ParcelaDto De(Parcela parcela)
    {
        ParcelaDto dto = new ParcelaDto();
        dto.Id = parcela.Id;
        dto.Numero = parcela.Numero;
        dto.Valor = parcela.Valor;
        dto.Vencimento = parcela.Vencimento;
        dto.PagoEm = parcela.PagoEm;
        dto.Status = "aberta";
        if (parcela.Paga)
            dto.Status = "paga";
        return dto;
    }
}

// O pedaço da viagem que aparece dentro da compra
public class ViagemResumoDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = "";
    public string Destino { get; set; } = "";
    public DateOnly DataSaida { get; set; }
    public string? Imagem { get; set; }

    public static ViagemResumoDto De(Viagem viagem, IEnderecoPublico endereco)
    {
        ViagemResumoDto dto = new ViagemResumoDto();
        dto.Id = viagem.Id;
        dto.Titulo = viagem.Titulo;
        dto.Destino = viagem.Destino;
        dto.DataSaida = viagem.DataSaida;

        foreach (ViagemImagem imagem in viagem.Imagens)
        {
            dto.Imagem = endereco.Completar(imagem.Url);   // só a primeira (capa)
            break;
        }
        return dto;
    }
}

public class CompraDto
{
    public int Id { get; set; }
    public string Metodo { get; set; } = "";
    public int NumParcelas { get; set; }
    public decimal ValorTotal { get; set; }
    public string Status { get; set; } = "";     // "pendente", "pago", "cancelado"
    public DateTime CriadoEm { get; set; }
    public ViagemResumoDto? Viagem { get; set; }
    public List<ParcelaDto> Parcelas { get; set; } = new List<ParcelaDto>();

    public static CompraDto De(Compra compra, IEnderecoPublico endereco)
    {
        CompraDto dto = new CompraDto();
        dto.Id = compra.Id;
        dto.Metodo = compra.Metodo;
        dto.NumParcelas = compra.Parcelas.Count;
        dto.ValorTotal = compra.ValorTotal;
        dto.Status = compra.Status.ToString().ToLowerInvariant();
        dto.CriadoEm = compra.CriadoEm;
        dto.Viagem = ViagemResumoDto.De(compra.Viagem, endereco);

        foreach (Parcela parcela in compra.Parcelas)
        {
            dto.Parcelas.Add(ParcelaDto.De(parcela));
        }
        return dto;
    }
}

public class PassageiroResumoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
}

// Uma linha da tela "Passageiros" do admin
public class PassageiroCompraDto
{
    public int CompraId { get; set; }
    public PassageiroResumoDto Passageiro { get; set; } = new PassageiroResumoDto();
    public string Metodo { get; set; } = "";
    public int NumParcelas { get; set; }
    public int ParcelasPagas { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal ValorPago { get; set; }
    public string Status { get; set; } = "";
    public DateTime CriadoEm { get; set; }

    public static PassageiroCompraDto De(Compra compra)
    {
        PassageiroCompraDto dto = new PassageiroCompraDto();
        dto.CompraId = compra.Id;
        dto.Passageiro.Id = compra.Passageiro.Id;
        dto.Passageiro.Nome = compra.Passageiro.Nome;
        dto.Passageiro.Email = compra.Passageiro.Email;
        dto.Metodo = compra.Metodo;
        dto.NumParcelas = compra.Parcelas.Count;
        dto.ParcelasPagas = compra.ParcelasPagas();
        dto.ValorTotal = compra.ValorTotal;
        dto.ValorPago = compra.ValorPago();
        dto.Status = compra.Status.ToString().ToLowerInvariant();
        dto.CriadoEm = compra.CriadoEm;
        return dto;
    }
}
