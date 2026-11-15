using Microsoft.AspNetCore.Mvc;
using Vendinha.Api.Dtos;
using Vendinha.Api.Negocio;

namespace Vendinha.Api.Controllers
{
    [ApiController]
    [Route("api/clientes")]
    public class ClientesController : ControllerBase
    {
        private readonly ClienteNegocio _negocio;

        public ClientesController(ClienteNegocio negocio)
        {
            _negocio = negocio;
        }

        [HttpGet]
        public ActionResult<ClientesPaginadosResponse> Listar([FromQuery] string? nome, [FromQuery] int pagina = 1)
        {
            return Ok(_negocio.Listar(nome, pagina));
        }

        [HttpGet("{id:int}")]
        public ActionResult<ClienteResponse> Obter(int id)
        {
            return Ok(_negocio.Obter(id));
        }

        [HttpPost]
        public ActionResult<ClienteResponse> Criar(ClienteRequest dados)
        {
            var cliente = _negocio.Criar(dados);
            return CreatedAtAction(nameof(Obter), new { id = cliente.Id }, cliente);
        }

        [HttpPut("{id:int}")]
        public ActionResult<ClienteResponse> Atualizar(int id, ClienteRequest dados)
        {
            return Ok(_negocio.Atualizar(id, dados));
        }

        [HttpDelete("{id:int}")]
        public IActionResult Excluir(int id)
        {
            _negocio.Excluir(id);
            return NoContent();
        }
    }
}
