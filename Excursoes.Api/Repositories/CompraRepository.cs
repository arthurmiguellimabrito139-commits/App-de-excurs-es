using Dapper;
using Excursoes.Api.Data;
using Excursoes.Api.Excecoes;
using Excursoes.Api.Models;
using MySqlConnector;

namespace Excursoes.Api.Repositories;

public interface ICompraRepository
{
    Task InserirAsync(Compra compra, Guid versaoAntigaDaViagem);
    Task<Compra?> BuscarPorParcelaAsync(int parcelaId);
    Task<List<Compra>> ListarDoPassageiroAsync(int passageiroId);   // "Minhas compras"
    Task<List<Compra>> ListarDaViagemAsync(int viagemId);            // tela "Passageiros" do admin
    Task SalvarPagamentoAsync(Compra compra);
}

public class CompraRepository : ICompraRepository
{
    private readonly ConexaoFactory _fabrica;

    public CompraRepository(ConexaoFactory fabrica)
    {
        _fabrica = fabrica;
    }

    // ==================== CREATE (a compra) ====================

    public async Task InserirAsync(Compra compra, Guid versaoAntigaDaViagem)
    {
        using MySqlConnection conexao = _fabrica.Criar();
        await conexao.OpenAsync();
        using MySqlTransaction transacao = await conexao.BeginTransactionAsync();

        // 1. "Reserva" a viagem: só passa se ninguém mexeu nela desde que foi lida.
        //    Se duas pessoas tentarem a última vaga ao mesmo tempo, só uma consegue.
        int linhas = await conexao.ExecuteAsync(
            "UPDATE Viagens SET Versao = @Nova WHERE Id = @ViagemId AND Versao = @Antiga",
            new { Nova = compra.Viagem.Versao, compra.ViagemId, Antiga = versaoAntigaDaViagem },
            transacao);

        if (linhas == 0)
            throw new ConflitoException("A viagem acabou de ser alterada (talvez a última vaga tenha sido vendida). Tente de novo.");

        // 2. A compra
        string sqlCompra = @"
            INSERT INTO Compras (PassageiroId, ViagemId, Metodo, ValorTotal, Status, CriadoEm)
            VALUES (@PassageiroId, @ViagemId, @Metodo, @ValorTotal, @Status, @CriadoEm);
            SELECT LAST_INSERT_ID();";

        int compraId = await conexao.ExecuteScalarAsync<int>(sqlCompra, new
        {
            compra.PassageiroId,
            compra.ViagemId,
            compra.Metodo,
            compra.ValorTotal,
            Status = compra.Status.ToString(),   // grava "Pendente", não o número 0
            compra.CriadoEm
        }, transacao);
        compra.DefinirId(compraId);

        // 3. As parcelas
        string sqlParcela = @"
            INSERT INTO Parcelas (CompraId, Numero, Valor, Vencimento, Paga, PagoEm)
            VALUES (@CompraId, @Numero, @Valor, @Vencimento, @Paga, @PagoEm);
            SELECT LAST_INSERT_ID();";

        foreach (Parcela parcela in compra.Parcelas)
        {
            int parcelaId = await conexao.ExecuteScalarAsync<int>(sqlParcela, new
            {
                CompraId = compraId,
                parcela.Numero,
                parcela.Valor,
                parcela.Vencimento,
                parcela.Paga,
                parcela.PagoEm
            }, transacao);
            parcela.DefinirId(parcelaId);
        }

        // Tudo junto ou nada: se qualquer passo falhar, o banco desfaz os anteriores
        await transacao.CommitAsync();
    }

    // ==================== READ ====================

    public async Task<Compra?> BuscarPorParcelaAsync(int parcelaId)
    {
        using MySqlConnection conexao = _fabrica.Criar();

        int? compraId = await conexao.ExecuteScalarAsync<int?>(
            "SELECT CompraId FROM Parcelas WHERE Id = @Id", new { Id = parcelaId });
        if (compraId == null)
            return null;

        Compra compra = await conexao.QueryFirstAsync<Compra>(
            "SELECT * FROM Compras WHERE Id = @Id", new { Id = compraId });
        IEnumerable<Parcela> parcelas = await conexao.QueryAsync<Parcela>(
            "SELECT * FROM Parcelas WHERE CompraId = @Id ORDER BY Numero", new { Id = compraId });

        compra.CarregarParcelas(parcelas);
        return compra;
    }

    public async Task<List<Compra>> ListarDoPassageiroAsync(int passageiroId)
    {
        using MySqlConnection conexao = _fabrica.Criar();

        IEnumerable<Compra> resultado = await conexao.QueryAsync<Compra>(
            "SELECT * FROM Compras WHERE PassageiroId = @Id ORDER BY CriadoEm DESC, Id DESC",
            new { Id = passageiroId });
        List<Compra> compras = resultado.AsList();

        if (compras.Count == 0)
            return compras;

        await CarregarParcelas(conexao, compras);

        // Cada compra precisa da viagem (título, destino, data, foto de capa)
        List<int> idsViagens = new List<int>();
        foreach (Compra compra in compras)
        {
            if (!idsViagens.Contains(compra.ViagemId))
                idsViagens.Add(compra.ViagemId);
        }

        IEnumerable<Viagem> viagens = await conexao.QueryAsync<Viagem>(
            "SELECT * FROM Viagens WHERE Id IN @Ids", new { Ids = idsViagens });
        IEnumerable<ViagemImagem> imagens = await conexao.QueryAsync<ViagemImagem>(
            "SELECT * FROM ViagemImagens WHERE ViagemId IN @Ids ORDER BY Id", new { Ids = idsViagens });

        Dictionary<int, Viagem> viagensPorId = new Dictionary<int, Viagem>();
        foreach (Viagem viagem in viagens)
        {
            viagem.CarregarImagens(FiltrarImagens(imagens, viagem.Id));
            viagensPorId[viagem.Id] = viagem;
        }

        foreach (Compra compra in compras)
        {
            compra.CarregarViagem(viagensPorId[compra.ViagemId]);
        }

        return compras;
    }

    public async Task<List<Compra>> ListarDaViagemAsync(int viagemId)
    {
        using MySqlConnection conexao = _fabrica.Criar();

        IEnumerable<Compra> resultado = await conexao.QueryAsync<Compra>(
            "SELECT * FROM Compras WHERE ViagemId = @Id ORDER BY CriadoEm, Id", new { Id = viagemId });
        List<Compra> compras = resultado.AsList();

        if (compras.Count == 0)
            return compras;

        await CarregarParcelas(conexao, compras);

        // Cada compra precisa do passageiro (nome e e-mail)
        List<int> idsPassageiros = new List<int>();
        foreach (Compra compra in compras)
        {
            if (!idsPassageiros.Contains(compra.PassageiroId))
                idsPassageiros.Add(compra.PassageiroId);
        }

        IEnumerable<Passageiro> passageiros = await conexao.QueryAsync<Passageiro>(
            "SELECT * FROM Usuarios WHERE Id IN @Ids AND Tipo = 'Passageiro'", new { Ids = idsPassageiros });

        Dictionary<int, Passageiro> passageirosPorId = new Dictionary<int, Passageiro>();
        foreach (Passageiro passageiro in passageiros)
        {
            passageirosPorId[passageiro.Id] = passageiro;
        }

        foreach (Compra compra in compras)
        {
            compra.CarregarPassageiro(passageirosPorId[compra.PassageiroId]);
        }

        return compras;
    }

    // ==================== UPDATE (pagamento) ====================

    public async Task SalvarPagamentoAsync(Compra compra)
    {
        using MySqlConnection conexao = _fabrica.Criar();
        await conexao.OpenAsync();
        using MySqlTransaction transacao = await conexao.BeginTransactionAsync();

        await conexao.ExecuteAsync(
            "UPDATE Compras SET Status = @Status WHERE Id = @Id",
            new { Status = compra.Status.ToString(), compra.Id }, transacao);

        foreach (Parcela parcela in compra.Parcelas)
        {
            await conexao.ExecuteAsync(
                "UPDATE Parcelas SET Paga = @Paga, PagoEm = @PagoEm WHERE Id = @Id",
                new { parcela.Paga, parcela.PagoEm, parcela.Id }, transacao);
        }

        await transacao.CommitAsync();
    }

    // ==================== Auxiliares ====================

    // Busca as parcelas de VÁRIAS compras numa consulta só e distribui
    private static async Task CarregarParcelas(MySqlConnection conexao, List<Compra> compras)
    {
        List<int> idsCompras = new List<int>();
        foreach (Compra compra in compras)
        {
            idsCompras.Add(compra.Id);
        }

        IEnumerable<Parcela> todas = await conexao.QueryAsync<Parcela>(
            "SELECT * FROM Parcelas WHERE CompraId IN @Ids ORDER BY Numero", new { Ids = idsCompras });

        foreach (Compra compra in compras)
        {
            List<Parcela> daCompra = new List<Parcela>();
            foreach (Parcela parcela in todas)
            {
                if (parcela.CompraId == compra.Id)
                    daCompra.Add(parcela);
            }
            compra.CarregarParcelas(daCompra);
        }
    }

    private static List<ViagemImagem> FiltrarImagens(IEnumerable<ViagemImagem> todas, int viagemId)
    {
        List<ViagemImagem> daViagem = new List<ViagemImagem>();
        foreach (ViagemImagem imagem in todas)
        {
            if (imagem.ViagemId == viagemId)
                daViagem.Add(imagem);
        }
        return daViagem;
    }
}
