using Excursoes.Api.Excecoes;
using Excursoes.Api.Models;
using Excursoes.Api.Strategies.Pagamento;

namespace Excursoes.Tests.Models;

public class CompraTests
{
    private static Compra CompraEm3x()
    {
        Viagem viagem = Fabrica.Viagem();
        Compra compra = Fabrica.Passageiro().Comprar(viagem, new BoletoStrategy(), 3);
        Fabrica.NumerarParcelas(compra);
        return compra;
    }

    [Fact]
    public void ValorTotal_EhASomaDasParcelas()
    {
        Compra compra = CompraEm3x();

        decimal soma = 0;
        foreach (Parcela parcela in compra.Parcelas)
        {
            soma = soma + parcela.Valor;
        }

        Assert.Equal(compra.ValorTotal, soma);
        Assert.Equal(StatusCompra.Pendente, compra.Status);
    }

    [Fact]
    public void PagarParcela_ForaDeOrdem_Recusa()
    {
        Compra compra = CompraEm3x();

        void Acao()
        {
            compra.PagarParcela(2);
        }

        RegraNegocioException erro = Assert.Throws<RegraNegocioException>(Acao);
        Assert.Equal("Pague antes a parcela 1.", erro.Message);
    }

    [Fact]
    public void PagarParcela_DuasVezes_Recusa()
    {
        Compra compra = CompraEm3x();
        compra.PagarParcela(1);

        void Acao()
        {
            compra.PagarParcela(1);
        }

        Assert.Throws<RegraNegocioException>(Acao);
    }

    [Fact]
    public void PagarParcela_QueNaoEhDestaCompra_NaoEncontra()
    {
        Compra compra = CompraEm3x();

        void Acao()
        {
            compra.PagarParcela(999);
        }

        Assert.Throws<NaoEncontradoException>(Acao);
    }

    [Fact]
    public void PagarTodas_CompraFicaQuitada()
    {
        Compra compra = CompraEm3x();

        compra.PagarParcela(1);
        compra.PagarParcela(2);
        compra.PagarParcela(3);

        Assert.Equal(StatusCompra.Pago, compra.Status);
        Assert.Equal(3, compra.ParcelasPagas());
        Assert.Equal(compra.ValorTotal, compra.ValorPago());
    }

    [Fact]
    public void Cancelar_CompraQuitada_Recusa()
    {
        Compra compra = Fabrica.Passageiro().Comprar(Fabrica.Viagem(), new PixStrategy(), 1);
        Fabrica.NumerarParcelas(compra);
        compra.PagarParcela(1);

        void Acao()
        {
            compra.Cancelar();
        }

        Assert.Throws<RegraNegocioException>(Acao);
    }

    [Fact]
    public void Cancelar_LiberaAVaga()
    {
        Viagem viagem = Fabrica.Viagem(vagas: 1);
        Compra compra = Fabrica.Passageiro().Comprar(viagem, new BoletoStrategy(), 2);
        Assert.Equal(0, viagem.VagasDisponiveis);

        compra.Cancelar();

        Assert.Equal(1, viagem.VagasDisponiveis);
    }
}
