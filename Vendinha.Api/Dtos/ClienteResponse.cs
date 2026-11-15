using Vendinha.Api.Models;

namespace Vendinha.Api.Dtos;

public class ClienteResponse
{
    public int Id { get; set; }
    public string NomeCompleto { get; set; } = "";
    public string Cpf { get; set; } = "";
    public DateOnly DataNascimento { get; set; }
    public int Idade { get; set; }
    public string? Email { get; set; }
    public decimal TotalDividas { get; set; }

    public static ClienteResponse DeCliente(Cliente cliente, long totalCentavos)
    {
        return new ClienteResponse
        {
            Id = cliente.Id,
            NomeCompleto = cliente.NomeCompleto,
            Cpf = cliente.Cpf,
            DataNascimento = cliente.DataNascimento,
            Idade = cliente.CalcularIdade(DateOnly.FromDateTime(DateTime.Today)),
            Email = cliente.Email,
            TotalDividas = totalCentavos / 100m
        };
    }
}
