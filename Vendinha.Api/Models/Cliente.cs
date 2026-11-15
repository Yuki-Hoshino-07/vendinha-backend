namespace Vendinha.Api.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string NomeCompleto { get; set; } = "";
        public string Cpf { get; set; } = "";
        public DateTime DataNascimento { get; set; }
        public string? Email { get; set; }
        public long TotalDividasCentavos { get; set; }

        public int Idade
        {
            get
            {
                int idade = DateTime.Today.Year - DataNascimento.Year;
                if (DataNascimento.Date.AddYears(idade) > DateTime.Today)
                {
                    idade--;
                }
                return idade;
            }
        }
    }
}
