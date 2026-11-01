using Excursoes.Api.Excecoes;
using Excursoes.Api.Models;
using Excursoes.Api.Strategies.Pagamento;

namespace Excursoes.Tests.Strategies;

public class PagamentoStrategyTests
{
    private static decimal Total(IReadOnlyList<ParcelaCalculada> parcelas)
    {
        decimal soma = 0;
        foreach (ParcelaCalculada parcela in parcelas)
        {
            soma = soma + parcela.Valor;
        }
        return soma;
    }

    [Fact]
    public void Pix_Da5PorCentoDeDesconto()
    {
        Viagem viagem = Fabrica.Viagem(Fabrica.Admin(), Fabrica.Dados(valor: 1890m));

        IReadOnlyList<ParcelaCalculada> parcelas = new PixStrategy().Calcular(viagem, 1);

        Assert.Single(parcelas);
        Assert.Equal(1795.50m, Total(parcelas));
    }

    [Fact]
    public void Pix_Parcelado_Recusa()
    {
        Viagem viagem = Fabrica.Viagem();

        void Acao()
        {
            new PixStrategy().Calcular(viagem, 2);
        }

        RegraNegocioException erro = Assert.Throws<RegraNegocioException>(Acao);
        Assert.Equal("Pix só aceita pagamento à vista.", erro.Message);
    }

    [Fact]
    public void Cartao_Ate3x_SemJuros()
    {
        Viagem viagem = Fabrica.Viagem(Fabrica.Admin(), Fabrica.Dados(valor: 1890m));

        Assert.Equal(1890m, Total(new CartaoStrategy().Calcular(viagem, 3)));
    }

    [Fact]
    public void Cartao_6x_ComJurosCompostos()
    {
        Viagem viagem = Fabrica.Viagem(Fabrica.Admin(), Fabrica.Dados(valor: 1890m));

        IReadOnlyList<ParcelaCalculada> parcelas = new CartaoStrategy().Calcular(viagem, 6);

        // 1890 x 1,025^6 = 2191,82
        Assert.Equal(2191.82m, Total(parcelas));
        Assert.Equal(6, parcelas.Count);
    }

    [Fact]
    public void Cartao_RespeitaOLimiteDoAdmin()
    {
        Viagem viagem = Fabrica.Viagem(Fabrica.Admin(), Fabrica.Dados(maxParcelas: 4));

        void Acao()
        {
            new CartaoStrategy().Calcular(viagem, 5);
        }

        Assert.Throws<RegraNegocioException>(Acao);
        Assert.Equal(4, new CartaoStrategy().MaxParcelas(viagem));
    }

    [Fact]
    public void Boleto_SemJuros()
    {
        Viagem viagem = Fabrica.Viagem(Fabrica.Admin(), Fabrica.Dados(valor: 1890m));

        Assert.Equal(1890m, Total(new BoletoStrategy().Calcular(viagem, 10)));
    }

    [Fact]
    public void Parcelas_UltimaFicaComOsCentavos()
    {
        Viagem viagem = Fabrica.Viagem(Fabrica.Admin(), Fabrica.Dados(valor: 1000m));

        IReadOnlyList<ParcelaCalculada> parcelas = new BoletoStrategy().Calcular(viagem, 3);

        Assert.Equal(333.33m, parcelas[0].Valor);
        Assert.Equal(333.33m, parcelas[1].Valor);
        Assert.Equal(333.34m, parcelas[2].Valor);
        Assert.Equal(1000m, Total(parcelas));
    }

    [Fact]
    public void Parcelas_VencemUmaPorMes()
    {
        Viagem viagem = Fabrica.Viagem();

        IReadOnlyList<ParcelaCalculada> parcelas = new BoletoStrategy().Calcular(viagem, 3);

        Assert.Equal(Fabrica.Hoje(), parcelas[0].Vencimento);
        Assert.Equal(Fabrica.Hoje().AddMonths(1), parcelas[1].Vencimento);
        Assert.Equal(Fabrica.Hoje().AddMonths(2), parcelas[2].Vencimento);
    }

    [Fact]
    public void Parcelas_ZeroOuNegativo_Recusa()
    {
        Viagem viagem = Fabrica.Viagem();

        void Acao()
        {
            new BoletoStrategy().Calcular(viagem, 0);
        }

        Assert.Throws<RegraNegocioException>(Acao);
    }

    [Fact]
    public void Context_EscolhePeloNome_ERecusaMetodoDesconhecido()
    {
        List<IPagamentoStrategy> estrategias = new List<IPagamentoStrategy>();
        estrategias.Add(new PixStrategy());
        estrategias.Add(new CartaoStrategy());
        estrategias.Add(new BoletoStrategy());
        PagamentoContext contexto = new PagamentoContext(estrategias);

        void Acao()
        {
            contexto.Obter("bitcoin");
        }

        Assert.Equal("cartao", contexto.Obter("cartao").Metodo);
        Assert.Equal(3, contexto.Todas().Count);
        Assert.Throws<RegraNegocioException>(Acao);
    }
}
