using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Sqlite;
using Vendinha.Api.Dados;
using Vendinha.Api.Dtos;
using Vendinha.Api.Models;
using Vendinha.Api.Validation;

namespace Vendinha.Api.Negocio
{
    public class ClienteNegocio
    {
        private readonly ClienteDados _dados;

        public ClienteNegocio(ClienteDados dados)
        {
            _dados = dados;
        }

        public ClientesPaginadosResponse Listar(string? nome, int pagina = 1)
        {
            if (pagina < 1 || pagina > 214748364)
            {
                throw new ErroNegocioException(TipoErroNegocio.Validacao, "Informe uma página válida, a partir de 1.");
            }

            int totalClientes;
            var clientes = _dados.Listar(nome?.Trim() ?? "", pagina, out totalClientes);
            var resposta = new ClientesPaginadosResponse { Pagina = pagina, TotalClientes = totalClientes };
            foreach (var cliente in clientes)
            {
                resposta.Clientes.Add(ClienteResponse.DeCliente(cliente));
            }
            return resposta;
        }

        public ClienteResponse Obter(int id)
        {
            var cliente = _dados.Obter(id);
            if (cliente == null)
            {
                throw new ErroNegocioException(TipoErroNegocio.NaoEncontrado, "Cliente não encontrado.");
            }
            return ClienteResponse.DeCliente(cliente);
        }

        public ClienteResponse Criar(ClienteRequest dados)
        {
            var cliente = ValidarCliente(dados);
            if (_dados.ExisteCpf(cliente.Cpf))
            {
                throw new ErroNegocioException(TipoErroNegocio.Conflito, "Já existe um cliente com este CPF.");
            }

            try
            {
                _dados.Inserir(cliente);
            }
            catch (SqliteException erro)
            {
                if (erro.SqliteExtendedErrorCode == 2067)
                {
                    throw new ErroNegocioException(TipoErroNegocio.Conflito, "Já existe um cliente com este CPF.");
                }
                throw;
            }
            return ClienteResponse.DeCliente(cliente);
        }

        public ClienteResponse Atualizar(int id, ClienteRequest dados)
        {
            var cliente = ValidarCliente(dados);
            Obter(id);
            cliente.Id = id;
            if (_dados.ExisteCpf(cliente.Cpf, id))
            {
                throw new ErroNegocioException(TipoErroNegocio.Conflito, "Já existe um cliente com este CPF.");
            }

            try
            {
                if (!_dados.Atualizar(cliente))
                {
                    throw new ErroNegocioException(TipoErroNegocio.NaoEncontrado, "Cliente não encontrado.");
                }
            }
            catch (SqliteException erro)
            {
                if (erro.SqliteExtendedErrorCode == 2067)
                {
                    throw new ErroNegocioException(TipoErroNegocio.Conflito, "Já existe um cliente com este CPF.");
                }
                throw;
            }
            return Obter(id);
        }

        public void Excluir(int id)
        {
            if (!_dados.Excluir(id))
            {
                throw new ErroNegocioException(TipoErroNegocio.NaoEncontrado, "Cliente não encontrado.");
            }
        }

        private static Cliente ValidarCliente(ClienteRequest dados)
        {
            if (string.IsNullOrWhiteSpace(dados.NomeCompleto) || dados.NomeCompleto.Length > 150)
            {
                throw new ErroNegocioException(TipoErroNegocio.Validacao, "Informe o nome completo com até 150 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(dados.Cpf) || !CpfValidator.EhValido(dados.Cpf))
            {
                throw new ErroNegocioException(TipoErroNegocio.Validacao, "Informe um CPF válido.");
            }
            if (!dados.DataNascimento.HasValue || dados.DataNascimento == DateOnly.MinValue ||
                dados.DataNascimento > DateOnly.FromDateTime(DateTime.Today))
            {
                throw new ErroNegocioException(TipoErroNegocio.Validacao, "Informe uma data de nascimento válida, que não esteja no futuro.");
            }
            if (dados.Email != null && dados.Email.Length > 254)
            {
                throw new ErroNegocioException(TipoErroNegocio.Validacao, "O e-mail deve ter até 254 caracteres.");
            }
            if (!string.IsNullOrWhiteSpace(dados.Email) && !new EmailAddressAttribute().IsValid(dados.Email.Trim()))
            {
                throw new ErroNegocioException(TipoErroNegocio.Validacao, "E-mail inválido.");
            }

            return new Cliente
            {
                NomeCompleto = dados.NomeCompleto.Trim(),
                Cpf = CpfValidator.Normalizar(dados.Cpf),
                DataNascimento = dados.DataNascimento.Value.ToDateTime(TimeOnly.MinValue),
                Email = string.IsNullOrWhiteSpace(dados.Email) ? null : dados.Email.Trim()
            };
        }
    }
}
