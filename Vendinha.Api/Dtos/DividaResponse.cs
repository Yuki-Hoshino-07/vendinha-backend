using Vendinha.Api.Models;

namespace Vendinha.Api.Dtos;

public class DividaResponse
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public decimal Valor { get; set; }
    public bool Paga { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataPagamento { get; set; }

    public static DividaResponse DeDivida(Divida divida)
    {
        return new DividaResponse
        {
            Id = divida.Id,
            ClienteId = divida.ClienteId,
            Valor = divida.ValorCentavos / 100m,
            Paga = divida.Paga,
            DataCriacao = DateTime.SpecifyKind(divida.DataCriacao, DateTimeKind.Utc),
            DataPagamento = divida.DataPagamento.HasValue
                ? DateTime.SpecifyKind(divida.DataPagamento.Value, DateTimeKind.Utc) : null
        };
    }
}
