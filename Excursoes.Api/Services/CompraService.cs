using Excursoes.Api.DTOs.Pagamentos;
using Excursoes.Api.Excecoes;
using Excursoes.Api.Models;
using Excursoes.Api.Repositories;
using Excursoes.Api.Strategies.Pagamento;

namespace Excursoes.Api.Services;

public interface ICompraService
{
    List<MetodoPagamentoDto> Metodos();
    Task<SimulacaoDto> SimularAsync(int viagemId, string? metodo, int numParcelas);
    Task<CompraDto> ComprarAsync(int viagemId, int passageiroId, CompraRequestDto dto);
    Task<List<CompraDto>> MinhasComprasAsync(int passageiroId);
    Task<ParcelaDto> PagarParcelaAsync(int parcelaId, int passageiroId);
}

public class CompraService : ICompraService
{
    private readonly IViagemRepository _viagens;
    private readonly IUsuarioRepository _usuarios;
    private readonly ICompraRepository _compras;
    private readonly PagamentoContext _pagamentos;
    private readonly IEnderecoPublico _endereco;
    private readonly bool _permitirPagamentoSimulado;

    public CompraService(IViagemRepository viagens, IUsuarioRepository usuarios, ICompraRepository compras,
                         PagamentoContext pagamentos, IEnderecoPublico endereco, IConfiguration configuracao)
    {
        _viagens = viagens;
        _usuarios = usuarios;
        _compras = compras;
        _pagamentos = pagamentos;
        _endereco = endereco;
        _permitirPagamentoSimulado = configuracao.GetValue<bool>("Pagamentos:PermitirPagamentoSimulado", false);
    }

    public List<MetodoPagamentoDto> Metodos()
    {
        List<MetodoPagamentoDto> metodos = new List<MetodoPagamentoDto>();
        foreach (IPagamentoStrategy strategy in _pagamentos.Todas())
        {
            metodos.Add(MetodoPagamentoDto.De(strategy));
        }
        return metodos;
    }

    // O cálculo SEMPRE é feito aqui (Strategy); o front só mostra
    public async Task<SimulacaoDto> SimularAsync(int viagemId, string? metodo, int numParcelas)
    {
        Viagem viagem = await BuscarViagem(viagemId);
        IPagamentoStrategy strategy = _pagamentos.Obter(metodo);
        IReadOnlyList<ParcelaCalculada> parcelas = strategy.Calcular(viagem, numParcelas);

        decimal total = 0;
        foreach (ParcelaCalculada parcela in parcelas)
        {
            total = total + parcela.Valor;
        }

        SimulacaoDto simulacao = new SimulacaoDto();
        simulacao.Metodo = strategy.Metodo;
        simulacao.NumParcelas = numParcelas;
        simulacao.MaxParcelas = strategy.MaxParcelas(viagem);
        simulacao.Total = total;
        simulacao.Parcelas = parcelas;
        return simulacao;
    }

    public async Task<CompraDto> ComprarAsync(int viagemId, int passageiroId, CompraRequestDto dto)
    {
        Passageiro? passageiro = await _usuarios.BuscarPassageiroAsync(passageiroId);
        if (passageiro == null)
            throw new NaoAutorizadoException("Sessão inválida. Entre novamente.");

        // O valor vem do BANCO, nunca do que o front mandou
        Viagem viagem = await BuscarViagem(viagemId);
        Guid versaoAntiga = viagem.Versao;   // guarda antes: o Vender gera uma versão nova

        IPagamentoStrategy strategy = _pagamentos.Obter(dto.Metodo);
        Compra compra = passageiro.Comprar(viagem, strategy, dto.NumParcelas);   // regras: models

        await _compras.InserirAsync(compra, versaoAntiga);   // transação + proteção da última vaga
        return CompraDto.De(compra, _endereco);
    }

    public async Task<List<CompraDto>> MinhasComprasAsync(int passageiroId)
    {
        List<Compra> compras = await _compras.ListarDoPassageiroAsync(passageiroId);
        List<CompraDto> resultado = new List<CompraDto>();
        foreach (Compra compra in compras)
        {
            resultado.Add(CompraDto.De(compra, _endereco));
        }
        return resultado;
    }

    // Pagamento SIMULADO (portfólio/demonstração). Com um gateway real (ex.: Mercado Pago),
    // quem marca a parcela como paga é o webhook do gateway, e isto fica desligado.
    public async Task<ParcelaDto> PagarParcelaAsync(int parcelaId, int passageiroId)
    {
        if (!_permitirPagamentoSimulado)
            throw new RegraNegocioException("Pagamento online indisponível no momento.");

        Compra? compra = await _compras.BuscarPorParcelaAsync(parcelaId);

        // Parcela de outra pessoa responde igual a "não existe" (404)
        if (compra == null || !compra.PertenceA(passageiroId))
            throw new NaoEncontradoException("Parcela não encontrada.");

        Parcela paga = compra.PagarParcela(parcelaId);   // regra "na ordem": model
        await _compras.SalvarPagamentoAsync(compra);
        return ParcelaDto.De(paga);
    }

    private async Task<Viagem> BuscarViagem(int id)
    {
        Viagem? viagem = await _viagens.BuscarAsync(id);
        if (viagem == null)
            throw new NaoEncontradoException("Viagem não encontrada.");
        return viagem;
    }
}
