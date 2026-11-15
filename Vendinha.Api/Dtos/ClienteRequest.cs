namespace Vendinha.Api.Dtos
{
    public class ClienteRequest
    {
        public string? NomeCompleto { get; set; }
        public string? Cpf { get; set; }
        public DateOnly? DataNascimento { get; set; }
        public string? Email { get; set; }
    }
}
