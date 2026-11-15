using Microsoft.Data.Sqlite;
using Vendinha.Api.Dados;
using Vendinha.Api.Dtos;
using Vendinha.Api.Models;

namespace Vendinha.Api.Negocio
{
    public class DividaNegocio
    {
        private readonly DividaDados _dividas;
        private readonly ClienteDados _clientes;

        public DividaNegocio(DividaDados dividas, ClienteDados clientes)
        {
            _dividas = dividas;
            _clientes = clientes;
        }

        public List<DividaResponse> Listar(int clienteId)
        {
            VerificarCliente(clienteId);
            var resposta = new List<DividaResponse>();
            foreach (var divida in _dividas.Listar(clienteId))
            {
                resposta.Add(DividaResponse.DeDivida(divida));
            }
            return resposta;
        }

        public DividaResponse Criar(int clienteId, DividaRequest dados)
        {
            if (!dados.Valor.HasValue || dados.Valor < 0.01m || dados.Valor > 999999999.99m)
            {
                throw new ErroNegocioException(TipoErroNegocio.Validacao, "O valor deve estar entre 0,01 e 999.999.999,99.");
            }
            if (decimal.Round(dados.Valor.Value, 2) != dados.Valor.Value)
            {
                throw new ErroNegocioException(TipoErroNegocio.Validacao, "O valor deve ter no máximo duas casas decimais.");
            }

            VerificarCliente(clienteId);
            if (_dividas.ExisteDividaAberta(clienteId))
            {
                throw new ErroNegocioException(TipoErroNegocio.Conflito, "Este cliente já possui uma dívida em aberto.");
            }

            var divida = new Divida
            {
                ClienteId = clienteId,
                ValorCentavos = (long)(dados.Valor.Value * 100m),
                Paga = false,
                DataCriacao = DateTime.UtcNow,
                DataPagamento = null
            };

            try
            {
                _dividas.Inserir(divida);
            }
            catch (SqliteException erro)
            {
                if (erro.SqliteExtendedErrorCode == 2067)
                {
                    throw new ErroNegocioException(TipoErroNegocio.Conflito, "Este cliente já possui uma dívida em aberto.");
                }
                if (erro.SqliteExtendedErrorCode == 787)
                {
                    throw new ErroNegocioException(TipoErroNegocio.NaoEncontrado, "Cliente não encontrado.");
                }
                throw;
            }
            return DividaResponse.DeDivida(divida);
        }

        public DividaResponse Pagar(int clienteId, int dividaId)
        {
            var divida = _dividas.RegistrarPagamento(clienteId, dividaId, DateTime.UtcNow);
            if (divida == null)
            {
                throw new ErroNegocioException(TipoErroNegocio.NaoEncontrado, "Dívida não encontrada para este cliente.");
            }
            return DividaResponse.DeDivida(divida);
        }

        private void VerificarCliente(int clienteId)
        {
            if (_clientes.Obter(clienteId) == null)
            {
                throw new ErroNegocioException(TipoErroNegocio.NaoEncontrado, "Cliente não encontrado.");
            }
        }
    }
}
