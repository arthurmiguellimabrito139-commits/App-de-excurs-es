using Excursoes.Api.Models;

namespace Excursoes.Tests;

// Cria objetos prontos para os testes (sem banco)
public static class Fabrica
{
    public static DateOnly Hoje()
    {
        return DateOnly.FromDateTime(DateTime.UtcNow);
    }

    public static DadosViagem Dados(decimal valor = 1000m, int vagas = 10, int maxParcelas = 10, int diasAteSaida = 30)
    {
        DateOnly saida = Hoje().AddDays(diasAteSaida);
        return new DadosViagem("Chapada Diamantina", "Lençóis, BA", "Quatro dias de trilhas.",
            saida, saida.AddDays(4), valor, vagas, maxParcelas);
    }

    public static Admin Admin(int id = 1)
    {
        Admin admin = new Admin("Ana Admin", "admin" + id + "@teste.com", "hash-qualquer");
        admin.DefinirId(id);
        return admin;
    }

    public static Passageiro Passageiro(int id = 10)
    {
        Passageiro passageiro = new Passageiro("Maria Passageira", "maria" + id + "@teste.com", "hash-qualquer");
        passageiro.DefinirId(id);
        return passageiro;
    }

    public static Viagem Viagem(Admin admin, DadosViagem dados, int id = 100)
    {
        Viagem viagem = admin.PublicarViagem(dados);
        viagem.DefinirId(id);
        return viagem;
    }

    public static Viagem Viagem(int vagas = 10)
    {
        return Viagem(Admin(), Dados(vagas: vagas));
    }

    // Dá ids às parcelas, como o banco faria
    public static void NumerarParcelas(Compra compra)
    {
        int id = 1;
        foreach (Parcela parcela in compra.Parcelas)
        {
            parcela.DefinirId(id);
            id++;
        }
    }
}
