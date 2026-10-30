using System.ComponentModel.DataAnnotations;

namespace Excursoes.Api.DTOs.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(150)]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Informe a senha.")]
    [StringLength(100)]
    public string Senha { get; set; } = "";
}