using Excursoes.Api.Configuracao;
using Excursoes.Api.DTOs.Auth;
using Excursoes.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Excursoes.Api.Controllers;

[Route("api/auth")]
[EnableRateLimiting(PoliticaDeLimites.Login)]   // poucas tentativas por minuto por IP
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthRespostaDto>> Login(LoginDto dto)
    {
        AuthRespostaDto resposta = await _authService.LoginAsync(dto);
        return Ok(resposta);
    }

    // Cadastro público: sempre cria Passageiro
    [HttpPost("cadastro")]
    public async Task<ActionResult<AuthRespostaDto>> Cadastrar(CadastroDto dto)
    {
        AuthRespostaDto resposta = await _authService.CadastrarAsync(dto);
        return Ok(resposta);
    }
}
