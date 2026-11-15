using Microsoft.Data.Sqlite;
using Vendinha.Api.Models;

namespace Vendinha.Api.Dados
{
    public class ClienteDados
    {
        private readonly BancoDados _banco;
        private const string ConsultaClientes = @"
            SELECT c.Id, c.NomeCompleto, c.Cpf, c.DataNascimento, c.Email,
                COALESCE((SELECT SUM(d.ValorCentavos) FROM Dividas d
                    WHERE d.ClienteId = c.Id AND d.Paga = 0), 0) AS TotalCentavos
            FROM Clientes c";

        public ClienteDados(BancoDados banco)
        {
            _banco = banco;
        }

        public List<Cliente> Listar(string nome, int pagina, out int totalClientes)
        {
            using var connection = _banco.CriarConexao();
            connection.Open();
            using var transaction = connection.BeginTransaction(deferred: true);

            try
            {
                string filtro = " WHERE instr(lower(c.NomeCompleto), lower(@Nome)) > 0";
                using var contador = new SqliteCommand("SELECT COUNT(*) FROM Clientes c" + filtro, connection, transaction);
                contador.Parameters.AddWithValue("@Nome", nome);
                totalClientes = Convert.ToInt32(contador.ExecuteScalar());

                string sql = ConsultaClientes + filtro + " ORDER BY TotalCentavos DESC, c.Id LIMIT 10 OFFSET @Inicio";
                using var command = new SqliteCommand(sql, connection, transaction);
                command.Parameters.AddWithValue("@Nome", nome);
                command.Parameters.AddWithValue("@Inicio", (pagina - 1) * 10);
                var clientes = new List<Cliente>();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clientes.Add(LerCliente(reader));
                    }
                }

                transaction.Commit();
                return clientes;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public Cliente? Obter(int id)
        {
            using var connection = _banco.CriarConexao();
            connection.Open();
            using var command = new SqliteCommand(ConsultaClientes + " WHERE c.Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return LerCliente(reader);
            }
            return null;
        }

        public bool ExisteCpf(string cpf, int idIgnorado = 0)
        {
            string sql = "SELECT COUNT(*) FROM Clientes WHERE Cpf = @Cpf AND Id <> @Id";
            using var connection = _banco.CriarConexao();
            connection.Open();
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@Cpf", cpf);
            command.Parameters.AddWithValue("@Id", idIgnorado);
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public void Inserir(Cliente cliente)
        {
            string sql = @"INSERT INTO Clientes (NomeCompleto, Cpf, DataNascimento, Email)
                VALUES (@Nome, @Cpf, @Nascimento, @Email)";
            using var connection = _banco.CriarConexao();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                using var command = new SqliteCommand(sql, connection, transaction);
                AdicionarParametros(command, cliente);
                command.ExecuteNonQuery();
                using var consultaId = new SqliteCommand("SELECT last_insert_rowid()", connection, transaction);
                cliente.Id = Convert.ToInt32(consultaId.ExecuteScalar());
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool Atualizar(Cliente cliente)
        {
            string sql = @"UPDATE Clientes SET NomeCompleto = @Nome, Cpf = @Cpf,
                DataNascimento = @Nascimento, Email = @Email WHERE Id = @Id";
            using var connection = _banco.CriarConexao();
            connection.Open();
            using var command = new SqliteCommand(sql, connection);
            AdicionarParametros(command, cliente);
            command.Parameters.AddWithValue("@Id", cliente.Id);
            return command.ExecuteNonQuery() > 0;
        }

        public bool Excluir(int id)
        {
            using var connection = _banco.CriarConexao();
            connection.Open();
            using var command = new SqliteCommand("DELETE FROM Clientes WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            return command.ExecuteNonQuery() > 0;
        }

        private static void AdicionarParametros(SqliteCommand command, Cliente cliente)
        {
            command.Parameters.AddWithValue("@Nome", cliente.NomeCompleto);
            command.Parameters.AddWithValue("@Cpf", cliente.Cpf);
            command.Parameters.AddWithValue("@Nascimento", cliente.DataNascimento.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@Email", (object?)cliente.Email ?? DBNull.Value);
        }

        private static Cliente LerCliente(SqliteDataReader reader)
        {
            return new Cliente
            {
                Id = reader.GetInt32(0),
                NomeCompleto = reader.GetString(1),
                Cpf = reader.GetString(2),
                DataNascimento = reader.GetDateTime(3),
                Email = reader.IsDBNull(4) ? null : reader.GetString(4),
                TotalDividasCentavos = reader.GetInt64(5)
            };
        }
    }
}
