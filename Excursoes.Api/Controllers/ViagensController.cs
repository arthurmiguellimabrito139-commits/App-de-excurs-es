using Excursoes.Api.Configuracao;
using Excursoes.Api.DTOs.Pagamentos;
using Excursoes.Api.DTOs.Viagens;
using Excursoes.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Excursoes.Api.Controllers;

[Route("api/viagens")]
public class ViagensController : ApiControllerBase
{
    private readonly IViagemService _viagemService;

    public ViagensController(IViagemService viagemService)
    {
        _viagemService = viagemService;
    }

    // GET api/viagens                   -> todas
    // GET api/viagens?disponiveis=true  -> só as à venda
    [HttpGet]
    public async Task<ActionResult<List<ViagemDto>>> Listar([FromQuery] bool disponiveis = false)
    {
        List<ViagemDto> viagens = await _viagemService.ListarAsync(disponiveis);
        return Ok(viagens);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ViagemDto>> Buscar(int id)
    {
        ViagemDto viagem = await _viagemService.BuscarAsync(id);
        return Ok(viagem);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(Seguranca.LimiteUploadBytes)]
    public async Task<ActionResult<ViagemDto>> Criar([FromForm] ViagemFormDto dto)
    {
        ViagemDto viagem = await _viagemService.CriarAsync(dto, UsuarioLogadoId());
        return CreatedAtAction(nameof(Buscar), new { id = viagem.Id }, viagem);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(Seguranca.LimiteUploadBytes)]
    public async Task<ActionResult<ViagemDto>> Atualizar(int id, [FromForm] ViagemFormDto dto)
    {
        ViagemDto viagem = await _viagemService.AtualizarAsync(id, dto, UsuarioLogadoId());
        return Ok(viagem);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Excluir(int id)
    {
        await _viagemService.ExcluirAsync(id, UsuarioLogadoId());
        return NoContent();
    }

    [HttpGet("{id:int}/passageiros")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<PassageiroCompraDto>>> Passageiros(int id)
    {
        List<PassageiroCompraDto> lista = await _viagemService.PassageirosAsync(id, UsuarioLogadoId());
        return Ok(lista);
    }
}
