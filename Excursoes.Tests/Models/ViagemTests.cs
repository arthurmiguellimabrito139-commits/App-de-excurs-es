using Excursoes.Api.Excecoes;
using Excursoes.Api.Models;
using Excursoes.Api.Strategies.Pagamento;

namespace Excursoes.Tests.Models;

public class ViagemTests
{
    [Fact]
    public void Publicar_ViagemValida_GuardaOsDados()
    {
        Admin admin = Fabrica.Admin(7);

        Viagem viagem = admin.PublicarViagem(Fabrica.Dados(valor: 1890.555m));

        Assert.Equal(7, viagem.AdminId);
        Assert.Equal("Chapada Diamantina", viagem.Titulo);
        Assert.Equal(1890.56m, viagem.Valor);   // arredonda para centavos
        Assert.Equal(10, viagem.VagasDisponiveis);
        Assert.NotEqual(Guid.Empty, viagem.Versao);
    }

    [Fact]
    public void Publicar_VoltaAntesDaIda_Recusa()
    {
        Admin admin = Fabrica.Admin();
        DateOnly ida = Fabrica.Hoje().AddDays(10);
        DadosViagem dados = new DadosViagem("Viagem teste", "Goiânia, GO", "", ida, ida.AddDays(-1), 500m, 10, 3);

        void Acao()
        {
            admin.PublicarViagem(dados);
        }

        RegraNegocioException erro = Assert.Throws<RegraNegocioException>(Acao);
        Assert.Equal("A volta não pode ser antes da ida.", erro.Message);
    }

    [Fact]
    public void Publicar_ValorZero_Recusa()
    {
        Admin admin = Fabrica.Admin();

        void Acao()
        {
            admin.PublicarViagem(Fabrica.Dados(valor: 0m));
        }

        Assert.Throws<RegraNegocioException>(Acao);
    }

    [Fact]
    public void Publicar_MaisDe12Parcelas_Recusa()
    {
        Admin admin = Fabrica.Admin();

        void Acao()
        {
            admin.PublicarViagem(Fabrica.Dados(maxParcelas: 13));
        }

        Assert.Throws<RegraNegocioException>(Acao);
    }

    [Fact]
    public void Atualizar_GeraNovaVersao()
    {
        Viagem viagem = Fabrica.Viagem();
        Guid antes = viagem.Versao;

        viagem.Atualizar(Fabrica.Dados(valor: 1200m));

        Assert.NotEqual(antes, viagem.Versao);
    }

    [Fact]
    public void Atualizar_VagasMenorQueAsVendidas_Recusa()
    {
        Viagem viagem = Fabrica.Viagem(vagas: 5);
        Fabrica.Passageiro(1).Comprar(viagem, new BoletoStrategy(), 1);
        Fabrica.Passageiro(2).Comprar(viagem, new BoletoStrategy(), 1);

        void Acao()
        {
            viagem.Atualizar(Fabrica.Dados(vagas: 1));
        }

        Assert.Throws<RegraNegocioException>(Acao);
    }

    [Fact]
    public void AdicionarImagem_MaisDe10_Recusa()
    {
        Viagem viagem = Fabrica.Viagem();
        for (int i = 0; i < Viagem.MaxImagens; i++)
        {
            viagem.AdicionarImagem("/uploads/foto" + i + ".jpg");
        }

        void Acao()
        {
            viagem.AdicionarImagem("/uploads/demais.jpg");
        }

        Assert.Throws<RegraNegocioException>(Acao);
    }

    [Fact]
    public void Vender_ViagemEsgotada_Recusa()
    {
        Viagem viagem = Fabrica.Viagem(vagas: 1);
        Fabrica.Passageiro(1).Comprar(viagem, new PixStrategy(), 1);

        void Acao()
        {
            Fabrica.Passageiro(2).Comprar(viagem, new PixStrategy(), 1);
        }

        RegraNegocioException erro = Assert.Throws<RegraNegocioException>(Acao);
        Assert.Equal("Viagem esgotada.", erro.Message);
        Assert.Equal(0, viagem.VagasDisponiveis);
    }

    [Fact]
    public void Vender_ViagemQueJaSaiu_Recusa()
    {
        Viagem viagem = Fabrica.Viagem(Fabrica.Admin(), Fabrica.Dados(diasAteSaida: -3));

        void Acao()
        {
            Fabrica.Passageiro().Comprar(viagem, new PixStrategy(), 1);
        }

        Assert.Throws<RegraNegocioException>(Acao);
        Assert.False(viagem.EstaAVenda(Fabrica.Hoje()));
    }

    [Fact]
    public void Vender_GeraNovaVersao_ParaProtegerAUltimaVaga()
    {
        Viagem viagem = Fabrica.Viagem();
        Guid antes = viagem.Versao;

        Fabrica.Passageiro().Comprar(viagem, new PixStrategy(), 1);

        Assert.NotEqual(antes, viagem.Versao);
    }

    [Fact]
    public void RemoverImagem_DevolveOEnderecoParaApagarOArquivo()
    {
        Viagem viagem = Fabrica.Viagem();
        ViagemImagem imagem = new ViagemImagem("/uploads/capa.jpg");
        imagem.DefinirId(5);
        List<ViagemImagem> imagens = new List<ViagemImagem>();
        imagens.Add(imagem);
        viagem.CarregarImagens(imagens);

        Assert.Equal("/uploads/capa.jpg", viagem.RemoverImagem(5));
        Assert.Null(viagem.RemoverImagem(5));
        Assert.Empty(viagem.Imagens);
    }
}
