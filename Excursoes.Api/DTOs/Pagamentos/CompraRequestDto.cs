using System.ComponentModel.DataAnnotations;

namespace Excursoes.Api.DTOs.Pagamentos;

public class CompraRequestDto
{
    [Required(ErrorMessage = "Escolha a forma de pagamento.")]
    [StringLength(20)]
    public string Metodo { get; set; } = "";

    [Range(1, 12, ErrorMessage = "Número de parcelas inválido.")]
    public int NumParcelas { get; set; }
}