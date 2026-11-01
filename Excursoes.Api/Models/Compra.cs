using Excursoes.Api.Excecoes;
using Excursoes.Api.Strategies.Pagamento;

namespace Excursoes.Api.Models;

public class Compra : Entidade
{
    private readonly List<Parcela> _parcelas = new List<Parcela>();

    public int PassageiroId { get; private set; }
    public Passageiro Passageiro { get; private set; } = null!;

    public int ViagemId { get; private set; }
    public Viagem Viagem { get; private set; } = null!;

    public string Metodo { get; private set; } = "";   // "pix", "cartao", "boleto"
    public decimal ValorTotal { get; private set; }
    public StatusCompra Status { get; private set; }
    public DateTime CriadoEm { get; private set; }

    public IReadOnlyCollection<Parcela> Parcelas
    {
        get { return _parcelas; }
    }

    // Compra cancelada não ocupa vaga
    public bool Ativa
    {
        get { return Status != StatusCompra.Cancelado; }
    }

    private Compra()
    {
    }

    // internal: só a Viagem.Vender cria compra
    internal Compra(Passageiro passageiro, Viagem viagem, string metodo,
                    IReadOnlyList<ParcelaCalculada> parcelasCalculadas)
    {
        if (parcelasCalculadas.Count == 0)
            throw new RegraNegocioException("A compra precisa ter pelo menos uma parcela.");

        Passageiro = passageiro;
        PassageiroId = passageiro.Id;
        Viagem = viagem;
        ViagemId = viagem.Id;
        Metodo = metodo;
        Status = StatusCompra.Pendente;
        CriadoEm = DateTime.UtcNow;

        decimal total = 0;
        foreach (ParcelaCalculada calculada in parcelasCalculadas)
        {
            _parcelas.Add(new Parcela(calculada.Numero, calculada.Valor, calculada.Vencimento));
            total = total + calculada.Valor;
        }
        ValorTotal = total;
    }

    // Paga uma parcela, sempre na ordem (1ª, 2ª, 3ª...)
    public Parcela PagarParcela(int parcelaId)
    {
        if (Status == StatusCompra.Cancelado)
            throw new RegraNegocioException("Esta compra foi cancelada.");

        Parcela? parcela = BuscarParcela(parcelaId);
        if (parcela == null)
            throw new NaoEncontradoException("Parcela não encontrada.");

        Parcela? anteriorEmAberto = PrimeiraEmAbertoAntesDe(parcela.Numero);
        if (anteriorEmAberto != null)
            throw new RegraNegocioException("Pague antes a parcela " + anteriorEmAberto.Numero + ".");

        parcela.MarcarComoPaga();

        if (TodasPagas())
            Status = StatusCompra.Pago;

        return parcela;
    }

    public void Cancelar()
    {
        if (Status == StatusCompra.Pago)
            throw new RegraNegocioException("Não é possível cancelar uma compra já quitada.");
        Status = StatusCompra.Cancelado;
    }

    public bool PertenceA(int passageiroId)
    {
        return PassageiroId == passageiroId;
    }

    // ---------- Informações para as telas ----------
    public int ParcelasPagas()
    {
        int quantidade = 0;
        foreach (Parcela parcela in _parcelas)
        {
            if (parcela.Paga)
                quantidade++;
        }
        return quantidade;
    }

    public decimal ValorPago()
    {
        decimal soma = 0;
        foreach (Parcela parcela in _parcelas)
        {
            if (parcela.Paga)
                soma = soma + parcela.Valor;
        }
        return soma;
    }

    // ---------- Carregamento vindo do banco ----------
    internal void CarregarParcelas(IEnumerable<Parcela> parcelas)
    {
        _parcelas.Clear();
        _parcelas.AddRange(parcelas);
    }

    internal void CarregarViagem(Viagem viagem)
    {
        Viagem = viagem;
    }

    internal void CarregarPassageiro(Passageiro passageiro)
    {
        Passageiro = passageiro;
    }

    // ---------- Auxiliares ----------
    private Parcela? BuscarParcela(int parcelaId)
    {
        foreach (Parcela parcela in _parcelas)
        {
            if (parcela.Id == parcelaId)
                return parcela;
        }
        return null;
    }

    private Parcela? PrimeiraEmAbertoAntesDe(int numero)
    {
        foreach (Parcela parcela in _parcelas)
        {
            if (parcela.Numero < numero && !parcela.Paga)
                return parcela;
        }
        return null;
    }

    private bool TodasPagas()
    {
        foreach (Parcela parcela in _parcelas)
        {
            if (!parcela.Paga)
                return false;
        }
        return true;
    }
}
