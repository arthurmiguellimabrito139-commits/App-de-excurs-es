using System.ComponentModel.DataAnnotations;

namespace Excursoes.Api.DTOs.Auth;

public class CadastroDto
{
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, MinimumLength = 2)]
    public string Nome { get; set; } = "";

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(150)]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Informe a senha.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha precisa ter pelo menos 6 caracteres.")]
    public string Senha { get; set; } = "";
}