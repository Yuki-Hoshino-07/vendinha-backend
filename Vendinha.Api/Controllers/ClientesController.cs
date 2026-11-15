using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Vendinha.Api.Data;
using Vendinha.Api.Dtos;
using Vendinha.Api.Models;
using Vendinha.Api.Validation;

namespace Vendinha.Api.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly VendinhaContext _banco;

    public ClientesController(VendinhaContext banco)
    {
        _banco = banco;
    }

    [HttpGet]
    public async Task<ActionResult<ClientesPaginadosResponse>> Listar(
        [FromQuery] string? nome, [FromQuery, Range(1, 214748364)] int pagina = 1)
    {
        var consulta = _banco.Clientes.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(nome))
        {
            string busca = nome.Trim().ToLower();
            consulta = consulta.Where(c => c.NomeCompleto.ToLower().Contains(busca));
        }

        int totalClientes = await consulta.CountAsync();
        var clientes = await consulta.Select(c => new
            {
                Cliente = c,
                TotalCentavos = c.Dividas.Where(d => !d.Paga).Sum(d => (long?)d.ValorCentavos) ?? 0
            })
            .OrderByDescending(c => c.TotalCentavos)
            .ThenBy(c => c.Cliente.Id)
            .Skip((pagina - 1) * 10).Take(10)
            .ToListAsync();

        return Ok(new ClientesPaginadosResponse
        {
            Pagina = pagina,
            TotalClientes = totalClientes,
            Clientes = clientes.Select(c => ClienteResponse.DeCliente(c.Cliente, c.TotalCentavos)).ToList()
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteResponse>> Obter(int id)
    {
        var cliente = await _banco.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (cliente == null)
            return NotFound(new { mensagem = "Cliente não encontrado." });

        long total = await _banco.Dividas.Where(d => d.ClienteId == id && !d.Paga)
            .SumAsync(d => (long?)d.ValorCentavos) ?? 0;
        return Ok(ClienteResponse.DeCliente(cliente, total));
    }

    [HttpPost]
    public async Task<ActionResult<ClienteResponse>> Criar(ClienteRequest dados)
    {
        string cpf = CpfValidator.Normalizar(dados.Cpf);
        if (await _banco.Clientes.AnyAsync(c => c.Cpf == cpf))
            return Conflict(new { mensagem = "Já existe um cliente com este CPF." });

        var cliente = new Cliente();
        PreencherCliente(cliente, dados);
        _banco.Clientes.Add(cliente);

        try
        {
            await _banco.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 })
        {
            return Conflict(new { mensagem = "Já existe um cliente com este CPF." });
        }

        return CreatedAtAction(nameof(Obter), new { id = cliente.Id }, ClienteResponse.DeCliente(cliente, 0));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClienteResponse>> Atualizar(int id, ClienteRequest dados)
    {
        var cliente = await _banco.Clientes.FindAsync(id);
        if (cliente == null)
            return NotFound(new { mensagem = "Cliente não encontrado." });

        string cpf = CpfValidator.Normalizar(dados.Cpf);
        if (await _banco.Clientes.AnyAsync(c => c.Cpf == cpf && c.Id != id))
            return Conflict(new { mensagem = "Já existe um cliente com este CPF." });

        PreencherCliente(cliente, dados);
        try
        {
            await _banco.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 })
        {
            return Conflict(new { mensagem = "Já existe um cliente com este CPF." });
        }

        long total = await _banco.Dividas.Where(d => d.ClienteId == id && !d.Paga)
            .SumAsync(d => (long?)d.ValorCentavos) ?? 0;
        return Ok(ClienteResponse.DeCliente(cliente, total));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var cliente = await _banco.Clientes.FindAsync(id);
        if (cliente == null)
            return NotFound(new { mensagem = "Cliente não encontrado." });

        _banco.Clientes.Remove(cliente);
        await _banco.SaveChangesAsync();
        return NoContent();
    }

    private static void PreencherCliente(Cliente cliente, ClienteRequest dados)
    {
        cliente.NomeCompleto = dados.NomeCompleto.Trim();
        cliente.Cpf = CpfValidator.Normalizar(dados.Cpf);
        cliente.DataNascimento = dados.DataNascimento!.Value;
        cliente.Email = string.IsNullOrWhiteSpace(dados.Email) ? null : dados.Email.Trim();
    }
}
