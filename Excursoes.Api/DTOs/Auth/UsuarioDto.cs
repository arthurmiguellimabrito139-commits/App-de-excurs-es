using Excursoes.Api.Models;

namespace Excursoes.Api.DTOs.Auth;

public record UsuarioDto(int Id, string Nome, string Email, string Tipo)
{
    public static UsuarioDto De(Usuario usuario)
    {
        return new UsuarioDto(usuario.Id, usuario.Nome, usuario.Email, usuario.Papel);
    }
}