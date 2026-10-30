using Excursoes.Api.Excecoes;
using Excursoes.Api.Strategies.Pagamento;

namespace Excursoes.Api.Models;

public class Passageiro : Usuario
{
    // TODO: lista privada de compras (mesmo esquema do _viagens no Admin)
    private readonly List<Compra> _compras = new();


    // TODO: expor as compras só para leitura (IReadOnlyCollection<Compra>)
    public IReadOnlyCollection<Compra> Compras => _compras;
    // TODO: sobrescrever o Papel → "Passageiro"
    public override string Papel => "Passageiro";
    // TODO: construtor vazio privado para o EF Core
    private Passageiro() { }
    // TODO: construtor público que repassa nome, email e senhaHash para o Usuario
    public Passageiro(string nome, string email, string senhaHash) : base(nome, email, senhaHash) { }
    /// <summary>
    /// Compra uma passagem da viagem com a forma de pagamento escolhida.
    /// </summary>
    public Compra Comprar(Viagem viagem, IPagamentoStrategy strategy, int numParcelas)
    {

        if (JaComprou(viagem))
            throw new RegraNegocioException("Você já comprou esta viagem.");

        // 2. a viagem confere a vaga e usa a strategy para gerar as parcelas
        var compra = viagem.Vender(this, strategy, numParcelas);

        // 3. guarda na lista do passageiro
        _compras.Add(compra);

        // 4. devolve para o service salvar e montar o DTO
        return compra;
        throw new NotImplementedException();
    }

    /// <summary>
    /// true se o passageiro tem uma compra ATIVA (não cancelada) desta viagem.
    /// </summary>
    public bool JaComprou(Viagem viagem)
    {

        {
            for (int i = 0; i < _compras.Count; i++)
            {
                if (_compras[i].ViagemId == viagem.Id && _compras[i].Ativa)
                    return true;
            }
            return false;
        }
        throw new NotImplementedException();
    }

    /// <summary>
    /// Paga uma parcela de UMA DAS COMPRAS DELE.
    /// </summary>
    public void PagarParcela(int compraId, int parcelaId)
    {
        Compra? compra = BuscarCompra(compraId);

        if (compra == null)
            throw new NaoEncontradoException("Compra não encontrada.");

        compra.PagarParcela(parcelaId);
    }

    private Compra? BuscarCompra(int compraId)
    {
        foreach (Compra compra in _compras)
        {
            if (compra.Id == compraId)
                return compra;
        }
        return null;
    }
}
