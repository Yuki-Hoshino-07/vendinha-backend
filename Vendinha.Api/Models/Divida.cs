namespace Vendinha.Api.Models;

public class Divida
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public long ValorCentavos { get; set; }
    public bool Paga { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataPagamento { get; set; }
}
