using Dapper;
using Excursoes.Api.Data;
using Excursoes.Api.Excecoes;
using Excursoes.Api.Models;
using MySqlConnector;

namespace Excursoes.Api.Repositories;

public interface IViagemRepository
{
    Task<List<Viagem>> ListarAsync();
    Task<Viagem?> BuscarAsync(int id);
    Task InserirAsync(Viagem viagem);
    Task AtualizarAsync(Viagem viagem, Guid versaoAntiga);
    Task ExcluirAsync(Viagem viagem);
}

public class ViagemRepository : IViagemRepository
{
    private const int ErroChaveEstrangeira = 1451;   // MySQL: linha usada por outra tabela

    private readonly ConexaoFactory _fabrica;

    public ViagemRepository(ConexaoFactory fabrica)
    {
        _fabrica = fabrica;
    }

    // ==================== READ ====================

    public async Task<List<Viagem>> ListarAsync()
    {
        using MySqlConnection conexao = _fabrica.Criar();

        IEnumerable<Viagem> viagens = await conexao.QueryAsync<Viagem>("SELECT * FROM Viagens ORDER BY DataSaida, Id");
        IEnumerable<ViagemImagem> imagens = await conexao.QueryAsync<ViagemImagem>("SELECT * FROM ViagemImagens ORDER BY Id");
        IEnumerable<Compra> compras = await conexao.QueryAsync<Compra>("SELECT * FROM Compras WHERE Status <> 'Cancelado'");

        // 3 consultas no total, em vez de 2 por viagem; depois distribui em memória
        List<Viagem> resultado = new List<Viagem>();
        foreach (Viagem viagem in viagens)
        {
            viagem.CarregarImagens(FiltrarImagens(imagens, viagem.Id));
            viagem.CarregarCompras(FiltrarCompras(compras, viagem.Id));
            resultado.Add(viagem);
        }
        return resultado;
    }

    public async Task<Viagem?> BuscarAsync(int id)
    {
        using MySqlConnection conexao = _fabrica.Criar();

        Viagem? viagem = await conexao.QueryFirstOrDefaultAsync<Viagem>(
            "SELECT * FROM Viagens WHERE Id = @Id", new { Id = id });
        if (viagem == null)
            return null;

        IEnumerable<ViagemImagem> imagens = await conexao.QueryAsync<ViagemImagem>(
            "SELECT * FROM ViagemImagens WHERE ViagemId = @Id ORDER BY Id", new { Id = id });
        IEnumerable<Compra> compras = await conexao.QueryAsync<Compra>(
            "SELECT * FROM Compras WHERE ViagemId = @Id AND Status <> 'Cancelado'", new { Id = id });

        viagem.CarregarImagens(imagens);
        viagem.CarregarCompras(compras);
        return viagem;
    }

    // ==================== CREATE ====================

    public async Task InserirAsync(Viagem viagem)
    {
        using MySqlConnection conexao = _fabrica.Criar();
        await conexao.OpenAsync();
        using MySqlTransaction transacao = await conexao.BeginTransactionAsync();

        string sql = @"
            INSERT INTO Viagens (AdminId, Titulo, Destino, Descricao, DataSaida, DataRetorno,
                                 Valor, Vagas, MaxParcelas, Versao)
            VALUES (@AdminId, @Titulo, @Destino, @Descricao, @DataSaida, @DataRetorno,
                    @Valor, @Vagas, @MaxParcelas, @Versao);
            SELECT LAST_INSERT_ID();";

        int novoId = await conexao.ExecuteScalarAsync<int>(sql, new
        {
            viagem.AdminId, viagem.Titulo, viagem.Destino, viagem.Descricao,
            viagem.DataSaida, viagem.DataRetorno, viagem.Valor,
            viagem.Vagas, viagem.MaxParcelas, viagem.Versao
        }, transacao);
        viagem.DefinirId(novoId);

        await InserirImagensNovas(conexao, transacao, viagem);
        await transacao.CommitAsync();
    }

    // ==================== UPDATE ====================

    public async Task AtualizarAsync(Viagem viagem, Guid versaoAntiga)
    {
        using MySqlConnection conexao = _fabrica.Criar();
        await conexao.OpenAsync();
        using MySqlTransaction transacao = await conexao.BeginTransactionAsync();

        string sql = @"
            UPDATE Viagens
               SET Titulo = @Titulo, Destino = @Destino, Descricao = @Descricao,
                   DataSaida = @DataSaida, DataRetorno = @DataRetorno, Valor = @Valor,
                   Vagas = @Vagas, MaxParcelas = @MaxParcelas, Versao = @Versao
             WHERE Id = @Id AND Versao = @VersaoAntiga";

        int linhas = await conexao.ExecuteAsync(sql, new
        {
            viagem.Id, viagem.Titulo, viagem.Destino, viagem.Descricao,
            viagem.DataSaida, viagem.DataRetorno, viagem.Valor,
            viagem.Vagas, viagem.MaxParcelas, viagem.Versao,
            VersaoAntiga = versaoAntiga
        }, transacao);

        // Nenhuma linha mudou = alguém alterou (ou vendeu) entre a leitura e agora
        if (linhas == 0)
            throw new ConflitoException("Esta viagem foi alterada enquanto você editava. Recarregue e tente de novo.");

        // Imagens: apaga as que saíram da lista e insere as novas
        List<int> idsQueFicam = new List<int>();
        foreach (ViagemImagem imagem in viagem.Imagens)
        {
            if (imagem.Id != 0)
                idsQueFicam.Add(imagem.Id);
        }

        if (idsQueFicam.Count == 0)
            await conexao.ExecuteAsync("DELETE FROM ViagemImagens WHERE ViagemId = @Id",
                new { viagem.Id }, transacao);
        else
            await conexao.ExecuteAsync("DELETE FROM ViagemImagens WHERE ViagemId = @Id AND Id NOT IN @Ids",
                new { viagem.Id, Ids = idsQueFicam }, transacao);

        await InserirImagensNovas(conexao, transacao, viagem);
        await transacao.CommitAsync();
    }

    // ==================== DELETE ====================

    public async Task ExcluirAsync(Viagem viagem)
    {
        using MySqlConnection conexao = _fabrica.Criar();
        try
        {
            // As imagens somem junto (ON DELETE CASCADE no schema.sql).
            // A Versao no WHERE impede apagar se alguém comprou depois da leitura.
            int linhas = await conexao.ExecuteAsync(
                "DELETE FROM Viagens WHERE Id = @Id AND Versao = @Versao",
                new { viagem.Id, viagem.Versao });

            if (linhas == 0)
                throw new ConflitoException("Esta viagem foi alterada. Recarregue e tente de novo.");
        }
        catch (MySqlException erro)
        {
            if (erro.Number == ErroChaveEstrangeira)
                throw new RegraNegocioException("Não é possível excluir: já existem passagens vendidas.");
            throw;
        }
    }

    // ==================== Auxiliares ====================

    private static async Task InserirImagensNovas(MySqlConnection conexao, MySqlTransaction transacao, Viagem viagem)
    {
        foreach (ViagemImagem imagem in viagem.Imagens)
        {
            if (imagem.Id != 0)
                continue;   // já está no banco

            int idImagem = await conexao.ExecuteScalarAsync<int>(
                "INSERT INTO ViagemImagens (ViagemId, Url) VALUES (@ViagemId, @Url); SELECT LAST_INSERT_ID();",
                new { ViagemId = viagem.Id, imagem.Url }, transacao);
            imagem.DefinirId(idImagem);
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

    private static List<Compra> FiltrarCompras(IEnumerable<Compra> todas, int viagemId)
    {
        List<Compra> daViagem = new List<Compra>();
        foreach (Compra compra in todas)
        {
            if (compra.ViagemId == viagemId)
                daViagem.Add(compra);
        }
        return daViagem;
    }
}
