using Microsoft.Data.Sqlite;

namespace Vendinha.Api.Dados
{
    public class BancoDados
    {
        private readonly string _connectionString;

        public BancoDados(IConfiguration configuration)
        {
            var configuracao = new SqliteConnectionStringBuilder(configuration.GetConnectionString("Vendinha"));
            configuracao.ForeignKeys = true;
            _connectionString = configuracao.ToString();
        }

        public SqliteConnection CriarConexao()
        {
            return new SqliteConnection(_connectionString);
        }

        public void CriarTabelas()
        {
            string caminho = Path.Combine(AppContext.BaseDirectory, "database", "schema.sql");
            string sql = File.ReadAllText(caminho);
            using var connection = CriarConexao();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                using var command = new SqliteCommand(sql, connection, transaction);
                command.ExecuteNonQuery();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
