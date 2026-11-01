using Dapper;
using Excursoes.Api.Data;
using Excursoes.Api.Excecoes;
using Excursoes.Api.Models;
using MySqlConnector;

namespace Excursoes.Api.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> BuscarPorEmailAsync(string email);   // login
    Task<Admin?> BuscarAdminAsync(int id);
    Task<Passageiro?> BuscarPassageiroAsync(int id);     // já vem com as compras
    Task<bool> EmailExisteAsync(string email);
    Task InserirAsync(Usuario usuario);
}

// Todo SQL usa parâmetros (@Email, @Id): o valor nunca é colado no texto do comando,
// então não existe injeção de SQL.
public class UsuarioRepository : IUsuarioRepository
{
    private const int ErroChaveDuplicada = 1062;   // código do MySQL para UNIQUE violado

    private readonly ConexaoFactory _fabrica;

    public UsuarioRepository(ConexaoFactory fabrica)
    {
        _fabrica = fabrica;
    }

    public async Task<Usuario?> BuscarPorEmailAsync(string email)
    {
        using MySqlConnection conexao = _fabrica.Criar();
        var parametros = new { Email = NormalizarEmail(email) };

        // 1º descobre o tipo, 2º monta o objeto da classe certa (herança numa tabela só)
        string? tipo = await conexao.ExecuteScalarAsync<string?>(
            "SELECT Tipo FROM Usuarios WHERE Email = @Email", parametros);

        if (tipo == null)
            return null;

        if (tipo == "Admin")
            return await conexao.QueryFirstAsync<Admin>("SELECT * FROM Usuarios WHERE Email = @Email", parametros);

        return await conexao.QueryFirstAsync<Passageiro>("SELECT * FROM Usuarios WHERE Email = @Email", parametros);
    }

    public async Task<Admin?> BuscarAdminAsync(int id)
    {
        using MySqlConnection conexao = _fabrica.Criar();
        return await conexao.QueryFirstOrDefaultAsync<Admin>(
            "SELECT * FROM Usuarios WHERE Id = @Id AND Tipo = 'Admin'", new { Id = id });
    }

    public async Task<Passageiro?> BuscarPassageiroAsync(int id)
    {
        using MySqlConnection conexao = _fabrica.Criar();

        Passageiro? passageiro = await conexao.QueryFirstOrDefaultAsync<Passageiro>(
            "SELECT * FROM Usuarios WHERE Id = @Id AND Tipo = 'Passageiro'", new { Id = id });
        if (passageiro == null)
            return null;

        // As compras são necessárias para a regra "já comprou esta viagem?"
        IEnumerable<Compra> compras = await conexao.QueryAsync<Compra>(
            "SELECT * FROM Compras WHERE PassageiroId = @Id", new { Id = id });
        passageiro.CarregarCompras(compras);

        return passageiro;
    }

    public async Task<bool> EmailExisteAsync(string email)
    {
        using MySqlConnection conexao = _fabrica.Criar();
        int quantidade = await conexao.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Usuarios WHERE Email = @Email", new { Email = NormalizarEmail(email) });
        return quantidade > 0;
    }

    public async Task InserirAsync(Usuario usuario)
    {
        using MySqlConnection conexao = _fabrica.Criar();

        string sql = @"
            INSERT INTO Usuarios (Tipo, Nome, Email, SenhaHash, CriadoEm)
            VALUES (@Tipo, @Nome, @Email, @SenhaHash, @CriadoEm);
            SELECT LAST_INSERT_ID();";

        try
        {
            int novoId = await conexao.ExecuteScalarAsync<int>(sql, new
            {
                Tipo = usuario.Papel,          // "Admin" ou "Passageiro"
                usuario.Nome,
                usuario.Email,
                usuario.SenhaHash,
                usuario.CriadoEm
            });
            usuario.DefinirId(novoId);
        }
        catch (MySqlException erro)
        {
            // Duas pessoas cadastrando o mesmo e-mail ao mesmo tempo: o UNIQUE do banco barra
            if (erro.Number == ErroChaveDuplicada)
                throw new RegraNegocioException("E-mail já cadastrado.");
            throw;
        }
    }

    private static string NormalizarEmail(string email)
    {
        if (email == null)
            return "";
        return email.Trim().ToLowerInvariant();
    }
}
