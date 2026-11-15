using Microsoft.AspNetCore.Mvc;
using Vendinha.Api.Dtos;
using Vendinha.Api.Negocio;

namespace Vendinha.Api.Controllers
{
    [ApiController]
    [Route("api/clientes/{clienteId:int}/dividas")]
    public class DividasController : ControllerBase
    {
        private readonly DividaNegocio _negocio;

        public DividasController(DividaNegocio negocio)
        {
            _negocio = negocio;
        }

        [HttpGet]
        public ActionResult<List<DividaResponse>> Listar(int clienteId)
        {
            return Ok(_negocio.Listar(clienteId));
        }

        [HttpPost]
        public ActionResult<DividaResponse> Criar(int clienteId, DividaRequest dados)
        {
            var divida = _negocio.Criar(clienteId, dados);
            return CreatedAtAction(nameof(Listar), new { clienteId }, divida);
        }

        [HttpPatch("{dividaId:int}/pagamento")]
        public ActionResult<DividaResponse> Pagar(int clienteId, int dividaId)
        {
            return Ok(_negocio.Pagar(clienteId, dividaId));
        }
    }
}
