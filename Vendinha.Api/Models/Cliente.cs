namespace Vendinha.Api.Models;

public class Cliente
{
    public int Id { get; set; }
    public string NomeCompleto { get; set; } = "";
    public string Cpf { get; set; } = "";
    public DateOnly DataNascimento { get; set; }
    public string? Email { get; set; }
    public List<Divida> Dividas { get; set; } = new();

    public int CalcularIdade(DateOnly hoje)
    {
        int idade = hoje.Year - DataNascimento.Year;
        if (DataNascimento.AddYears(idade) > hoje)
            idade--;

        return idade;
    }
}
