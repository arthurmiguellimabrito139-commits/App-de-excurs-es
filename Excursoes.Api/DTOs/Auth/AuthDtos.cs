using System.ComponentModel.DataAnnotations;
using Excursoes.Api.Models;

namespace Excursoes.Api.DTOs.Auth;

// ---------- Entrada ----------

public class LoginDto
{
    [Required(ErrorMessage = "Informe o e-mail.")]
    [StringLength(150, ErrorMessage = "E-mail muito longo.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Informe a senha.")]
    [StringLength(72, ErrorMessage = "Senha muito longa.")]
    public string Senha { get; set; } = "";
}

// Não existe campo "tipo": o cadastro público SEMPRE cria Passageiro.
public class CadastroDto
{
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
    public string Nome { get; set; } = "";

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(150, ErrorMessage = "E-mail muito longo.")]
    public string Email { get; set; } = "";

    // BCrypt só considera os primeiros 72 bytes, por isso o limite
    [Required(ErrorMessage = "Informe a senha.")]
    [StringLength(72, MinimumLength = 8, ErrorMessage = "A senha precisa ter entre 8 e 72 caracteres.")]
    public string Senha { get; set; } = "";
}

// ---------- Saída ----------

// Nunca inclui a senha
public class UsuarioDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public string Tipo { get; set; } = "";

    public static UsuarioDto De(Usuario usuario)
    {
        UsuarioDto dto = new UsuarioDto();
        dto.Id = usuario.Id;
        dto.Nome = usuario.Nome;
        dto.Email = usuario.Email;
        dto.Tipo = usuario.Papel;
        return dto;
    }
}

public class AuthRespostaDto
{
    public string Token { get; set; } = "";
    public UsuarioDto Usuario { get; set; } = new UsuarioDto();
}
