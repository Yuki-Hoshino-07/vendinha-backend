using Vendinha.Api.Models;

namespace Vendinha.Api.Dtos
{
    public class ClienteResponse
    {
        public int Id { get; set; }
        public string NomeCompleto { get; set; } = "";
        public string Cpf { get; set; } = "";
        public DateOnly DataNascimento { get; set; }
        public int Idade { get; set; }
        public string? Email { get; set; }
        public decimal TotalDividas { get; set; }

        public static ClienteResponse DeCliente(Cliente cliente)
        {
            return new ClienteResponse
            {
                Id = cliente.Id,
                NomeCompleto = cliente.NomeCompleto,
                Cpf = cliente.Cpf,
                DataNascimento = DateOnly.FromDateTime(cliente.DataNascimento),
                Idade = cliente.Idade,
                Email = cliente.Email,
                TotalDividas = cliente.TotalDividasCentavos / 100m
            };
        }
    }
}
