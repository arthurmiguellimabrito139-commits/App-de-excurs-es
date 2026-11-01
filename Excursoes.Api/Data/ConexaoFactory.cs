using MySqlConnector;

namespace Excursoes.Api.Data;

// Cria as conexões com o MySQL. Os repositórios pedem uma conexão aqui
// em vez de cada um ler a configuração por conta própria.
public class ConexaoFactory
{
    private readonly string _stringConexao;

    public ConexaoFactory(IConfiguration configuracao)
    {
        string? valor = configuracao.GetConnectionString("Padrao");
        if (string.IsNullOrWhiteSpace(valor))
            throw new InvalidOperationException(
                "Connection string 'Padrao' não configurada. Use: dotnet user-secrets set \"ConnectionStrings:Padrao\" \"...\"");

        MySqlConnectionStringBuilder construtor = new MySqlConnectionStringBuilder(valor);
        construtor.DateTimeKind = MySqlDateTimeKind.Utc;   // datas voltam do banco marcadas como UTC
        construtor.GuidFormat = MySqlGuidFormat.Char36;    // a coluna Versao é CHAR(36)
        _stringConexao = construtor.ConnectionString;
    }

    public MySqlConnection Criar()
    {
        return new MySqlConnection(_stringConexao);
    }
}
