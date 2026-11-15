namespace Vendinha.Api.Dtos
{
    public class ClientesPaginadosResponse
    {
        public int Pagina { get; set; }
        public int TamanhoPagina { get; set; } = 10;
        public int TotalClientes { get; set; }
        public int TotalPaginas
        {
            get
            {
                return (int)Math.Ceiling(TotalClientes / (double)TamanhoPagina);
            }
        }
        public List<ClienteResponse> Clientes { get; set; } = new List<ClienteResponse>();
    }
}
