using Excursoes.Api.DTOs.Pagamentos;
using Excursoes.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Excursoes.Api.Controllers;

public class PagamentosController : ApiControllerBase
{
    private readonly ICompraService _compraService;

    public PagamentosController(ICompraService compraService)
    {
        _compraService = compraService;
    }

    // Lista as strategies registradas, para o front montar as opções
    [HttpGet("api/pagamentos/metodos")]
    public ActionResult<List<MetodoPagamentoDto>> Metodos()
    {
        return Ok(_compraService.Metodos());
    }

    // GET api/viagens/1/simular?metodo=cartao&parcelas=6
    [HttpGet("api/viagens/{viagemId:int}/simular")]
    public async Task<ActionResult<SimulacaoDto>> Simular(int viagemId,
        [FromQuery] string? metodo, [FromQuery] int parcelas = 1)
    {
        SimulacaoDto simulacao = await _compraService.SimularAsync(viagemId, metodo, parcelas);
        return Ok(simulacao);
    }

    // O valor NÃO vem do front: o service busca no banco
    [HttpPost("api/viagens/{viagemId:int}/comprar")]
    [Authorize(Roles = "Passageiro")]
    public async Task<ActionResult<CompraDto>> Comprar(int viagemId, CompraRequestDto dto)
    {
        CompraDto compra = await _compraService.ComprarAsync(viagemId, UsuarioLogadoId(), dto);
        return Ok(compra);
    }
}
