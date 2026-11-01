using Excursoes.Api.DTOs.Pagamentos;
using Excursoes.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Excursoes.Api.Controllers;

[Authorize(Roles = "Passageiro")]   // vale para todos os métodos da classe
public class ComprasController : ApiControllerBase
{
    private readonly ICompraService _compraService;

    public ComprasController(ICompraService compraService)
    {
        _compraService = compraService;
    }

    // Só as compras de quem está logado (o id vem do token)
    [HttpGet("api/compras/minhas")]
    public async Task<ActionResult<List<CompraDto>>> MinhasCompras()
    {
        List<CompraDto> compras = await _compraService.MinhasComprasAsync(UsuarioLogadoId());
        return Ok(compras);
    }

    // Pagamento SIMULADO (liga/desliga em Pagamentos:PermitirPagamentoSimulado).
    // Com gateway real, quem marca a parcela como paga é o webhook do gateway.
    [HttpPost("api/parcelas/{parcelaId:int}/pagar")]
    public async Task<ActionResult<ParcelaDto>> PagarParcela(int parcelaId)
    {
        ParcelaDto parcela = await _compraService.PagarParcelaAsync(parcelaId, UsuarioLogadoId());
        return Ok(parcela);
    }
}
