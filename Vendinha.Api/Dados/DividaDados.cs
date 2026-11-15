using Microsoft.Data.Sqlite;
using Vendinha.Api.Models;

namespace Vendinha.Api.Dados
{
    public class DividaDados
    {
        private readonly BancoDados _banco;
        private const string ConsultaDividas = @"SELECT Id, ClienteId, ValorCentavos, Paga,
            DataCriacao, DataPagamento FROM Dividas";

        public DividaDados(BancoDados banco)
        {
            _banco = banco;
        }

        public List<Divida> Listar(int clienteId)
        {
            string sql = ConsultaDividas + " WHERE ClienteId = @ClienteId ORDER BY DataCriacao DESC, Id DESC";
            using var connection = _banco.CriarConexao();
            connection.Open();
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@ClienteId", clienteId);
            using var reader = command.ExecuteReader();
            var dividas = new List<Divida>();
            while (reader.Read())
            {
                dividas.Add(LerDivida(reader));
            }
            return dividas;
        }

        public bool ExisteDividaAberta(int clienteId)
        {
            string sql = "SELECT COUNT(*) FROM Dividas WHERE ClienteId = @ClienteId AND Paga = 0";
            using var connection = _banco.CriarConexao();
            connection.Open();
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@ClienteId", clienteId);
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public void Inserir(Divida divida)
        {
            string sql = @"INSERT INTO Dividas (ClienteId, ValorCentavos, Paga, DataCriacao, DataPagamento)
                VALUES (@ClienteId, @Valor, @Paga, @Criacao, @Pagamento)";
            using var connection = _banco.CriarConexao();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                using var command = new SqliteCommand(sql, connection, transaction);
                command.Parameters.AddWithValue("@ClienteId", divida.ClienteId);
                command.Parameters.AddWithValue("@Valor", divida.ValorCentavos);
                command.Parameters.AddWithValue("@Paga", divida.Paga);
                command.Parameters.AddWithValue("@Criacao", divida.DataCriacao);
                command.Parameters.AddWithValue("@Pagamento", (object?)divida.DataPagamento ?? DBNull.Value);
                command.ExecuteNonQuery();
                using var consultaId = new SqliteCommand("SELECT last_insert_rowid()", connection, transaction);
                divida.Id = Convert.ToInt32(consultaId.ExecuteScalar());
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public Divida? RegistrarPagamento(int clienteId, int dividaId, DateTime dataPagamento)
        {
            using var connection = _banco.CriarConexao();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                string sql = @"UPDATE Dividas SET Paga = 1, DataPagamento = @Pagamento
                    WHERE Id = @Id AND ClienteId = @ClienteId AND Paga = 0";
                using var command = new SqliteCommand(sql, connection, transaction);
                command.Parameters.AddWithValue("@Pagamento", dataPagamento);
                command.Parameters.AddWithValue("@Id", dividaId);
                command.Parameters.AddWithValue("@ClienteId", clienteId);
                command.ExecuteNonQuery();

                string consulta = ConsultaDividas + " WHERE Id = @Id AND ClienteId = @ClienteId";
                using var select = new SqliteCommand(consulta, connection, transaction);
                select.Parameters.AddWithValue("@Id", dividaId);
                select.Parameters.AddWithValue("@ClienteId", clienteId);
                Divida? divida = null;
                using (var reader = select.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        divida = LerDivida(reader);
                    }
                }
                transaction.Commit();
                return divida;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static Divida LerDivida(SqliteDataReader reader)
        {
            return new Divida
            {
                Id = reader.GetInt32(0),
                ClienteId = reader.GetInt32(1),
                ValorCentavos = reader.GetInt64(2),
                Paga = reader.GetBoolean(3),
                DataCriacao = reader.GetDateTime(4),
                DataPagamento = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
            };
        }
    }
}
