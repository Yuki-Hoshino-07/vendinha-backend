using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Vendinha.Api.Data;
using Vendinha.Api.Dtos;
using Vendinha.Api.Models;

namespace Vendinha.Api.Controllers;

[ApiController]
[Route("api/clientes/{clienteId:int}/dividas")]
public class DividasController : ControllerBase
{
    private readonly VendinhaContext _banco;

    public DividasController(VendinhaContext banco)
    {
        _banco = banco;
    }

    [HttpGet]
    public async Task<ActionResult<List<DividaResponse>>> Listar(int clienteId)
    {
        if (!await _banco.Clientes.AnyAsync(c => c.Id == clienteId))
            return NotFound(new { mensagem = "Cliente não encontrado." });

        var dividas = await _banco.Dividas.AsNoTracking().Where(d => d.ClienteId == clienteId)
            .OrderByDescending(d => d.DataCriacao).ThenByDescending(d => d.Id).ToListAsync();
        return Ok(dividas.Select(DividaResponse.DeDivida).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<DividaResponse>> Criar(int clienteId, DividaRequest dados)
    {
        if (!await _banco.Clientes.AnyAsync(c => c.Id == clienteId))
            return NotFound(new { mensagem = "Cliente não encontrado." });

        if (await _banco.Dividas.AnyAsync(d => d.ClienteId == clienteId && !d.Paga))
            return Conflict(new { mensagem = "Este cliente já possui uma dívida em aberto." });

        var divida = new Divida
        {
            ClienteId = clienteId,
            ValorCentavos = (long)(dados.Valor!.Value * 100m),
            Paga = false,
            DataCriacao = DateTime.UtcNow
        };
        _banco.Dividas.Add(divida);

        try
        {
            await _banco.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 })
        {
            return Conflict(new { mensagem = "Este cliente já possui uma dívida em aberto." });
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteExtendedErrorCode: 787 })
        {
            return NotFound(new { mensagem = "Cliente não encontrado." });
        }

        return CreatedAtAction(nameof(Listar), new { clienteId }, DividaResponse.DeDivida(divida));
    }

    [HttpPatch("{dividaId:int}/pagamento")]
    public async Task<ActionResult<DividaResponse>> Pagar(int clienteId, int dividaId)
    {
        await _banco.Dividas.Where(d => d.Id == dividaId && d.ClienteId == clienteId && !d.Paga)
            .ExecuteUpdateAsync(atualizacao => atualizacao
                .SetProperty(d => d.Paga, true)
                .SetProperty(d => d.DataPagamento, DateTime.UtcNow));

        var divida = await _banco.Dividas.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == dividaId && d.ClienteId == clienteId);
        if (divida == null)
            return NotFound(new { mensagem = "Dívida não encontrada para este cliente." });

        return Ok(DividaResponse.DeDivida(divida));
    }
}
