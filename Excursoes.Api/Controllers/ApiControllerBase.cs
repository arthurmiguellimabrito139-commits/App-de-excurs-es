using System.Security.Claims;
using Excursoes.Api.Excecoes;
using Excursoes.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Excursoes.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    // O id SEMPRE vem do token assinado, nunca do corpo da requisição ou da URL
    protected int UsuarioLogadoId()
    {
        Claim? claim = User.FindFirst(TokenService.ClaimId);
        if (claim == null || !int.TryParse(claim.Value, out int id))
            throw new NaoAutorizadoException("Faça login para continuar.");
        return id;
    }
}
