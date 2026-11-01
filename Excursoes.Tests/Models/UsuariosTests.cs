using Excursoes.Api.Excecoes;
using Excursoes.Api.Models;
using Excursoes.Api.Strategies.Pagamento;

namespace Excursoes.Tests.Models;

public class UsuariosTests
{
    [Fact]
    public void Email_EhGuardadoEmMinusculasESemEspacos()
    {
        Passageiro passageiro = new Passageiro("  Maria  ", "  Maria@Email.COM ", "hash");

        Assert.Equal("maria@email.com", passageiro.Email);
        Assert.Equal("Maria", passageiro.Nome);
        Assert.Equal("Passageiro", passageiro.Papel);
    }

    [Fact]
    public void Email_Invalido_Recusa()
    {
        void Acao()
        {
            new Passageiro("Maria", "isso-nao-e-email", "hash");
        }

        Assert.Throws<RegraNegocioException>(Acao);
    }

    [Fact]
    public void Nome_Curto_Recusa()
    {
        void Acao()
        {
            new Admin("A", "a@teste.com", "hash");
        }

        Assert.Throws<RegraNegocioException>(Acao);
    }

    [Fact]
    public void Passageiro_ComprarAMesmaViagemDuasVezes_Recusa()
    {
        Viagem viagem = Fabrica.Viagem();
        Passageiro passageiro = Fabrica.Passageiro();
        passageiro.Comprar(viagem, new PixStrategy(), 1);

        void Acao()
        {
            passageiro.Comprar(viagem, new BoletoStrategy(), 2);
        }

        RegraNegocioException erro = Assert.Throws<RegraNegocioException>(Acao);
        Assert.Equal("Você já comprou esta viagem.", erro.Message);
        Assert.Single(passageiro.Compras);
    }

    [Fact]
    public void Admin_EditarViagemDeOutroAdmin_NaoEncontra()
    {
        Admin dono = Fabrica.Admin(1);
        Admin outro = Fabrica.Admin(2);
        Viagem viagem = Fabrica.Viagem(dono, Fabrica.Dados());

        void Acao()
        {
            outro.EditarViagem(viagem, Fabrica.Dados(valor: 1m));
        }

        Assert.Throws<NaoEncontradoException>(Acao);
        Assert.Equal(1000m, viagem.Valor);   // nada mudou
    }

    [Fact]
    public void Admin_ExcluirViagemComVendas_Recusa()
    {
        Admin dono = Fabrica.Admin(1);
        Viagem viagem = Fabrica.Viagem(dono, Fabrica.Dados());
        Fabrica.Passageiro().Comprar(viagem, new PixStrategy(), 1);

        void Acao()
        {
            dono.AutorizarExclusao(viagem);
        }

        Assert.Throws<RegraNegocioException>(Acao);
    }

    [Fact]
    public void Admin_ExcluirViagemSemVendas_Permite()
    {
        Admin dono = Fabrica.Admin(1);
        Viagem viagem = Fabrica.Viagem(dono, Fabrica.Dados());

        dono.AutorizarExclusao(viagem);

        Assert.True(viagem.PodeExcluir);
    }
}
