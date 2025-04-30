using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRentalApp.Data;
using MotoRentalApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MotoRentalApp.Services;

namespace MotoRentalApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MotosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IRabbitMQService _rabbitMQService;

        public MotosController(AppDbContext context, IRabbitMQService rabbitMQService)
        {
            _context = context;
            _rabbitMQService = rabbitMQService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMoto([FromBody] Moto moto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { mensagem = "Dados inválidos" });

            var existingMoto = await _context.Motos.FirstOrDefaultAsync(m => m.Placa == moto.Placa);
            if (existingMoto != null)
                return BadRequest(new { mensagem = "Placa já cadastrada" });

            _context.Motos.Add(moto);
            await _context.SaveChangesAsync();

            var message = System.Text.Json.JsonSerializer.Serialize(new
            {
                Event = "MotoCadastrada",
                MotoId = moto.Identificador,
                Placa = moto.Placa,
                Modelo = moto.Modelo,
                Ano = moto.Ano
            });

            _rabbitMQService.SendMessage(message);

            return CreatedAtAction(nameof(GetMotoById), new { id = moto.Identificador }, moto);
        }

        [HttpGet]
        public async Task<ActionResult> GetMotos(
            [FromQuery] string? placa,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
                return BadRequest(new { mensagem = "Página e tamanho da página devem ser positivos." });

            var query = _context.Motos.AsQueryable();

            if (!string.IsNullOrEmpty(placa))
                query = query.Where(m => m.Placa.ToLower() == placa.ToLower());

            var totalItems = await query.CountAsync();
            var motos = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                totalItems,
                page,
                pageSize,
                items = motos
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMotoById(string id)
        {
            var moto = await _context.Motos.FindAsync(id);
            if (moto == null)
                return NotFound(new { mensagem = "Moto não encontrada" });

            return Ok(moto);
        }

        [HttpPut("{id}/placa")]
        public async Task<IActionResult> UpdatePlaca(string id, [FromBody] Moto motoUpdate)
        {
            if (!ModelState.IsValid || motoUpdate == null || string.IsNullOrEmpty(motoUpdate.Placa))
                return BadRequest(new { mensagem = "Dados inválidos" });

            var moto = await _context.Motos.FindAsync(id);
            if (moto == null)
                return NotFound(new { mensagem = "Moto não encontrada" });

            var existingMotoWithPlaca = await _context.Motos.FirstOrDefaultAsync(m => m.Placa == motoUpdate.Placa);
            if (existingMotoWithPlaca != null && existingMotoWithPlaca.Identificador != id)
                return BadRequest(new { mensagem = "Placa já cadastrada" });

            moto.Placa = motoUpdate.Placa;
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Placa modificada com sucesso" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMoto(string id)
        {
            var moto = await _context.Motos.FindAsync(id);
            if (moto == null)
                return BadRequest(new { mensagem = "Dados inválidos" });

            var hasLocacoes = await _context.Locacoes.AnyAsync(l => l.MotoId == id);
            if (hasLocacoes)
                return BadRequest(new { mensagem = "Não é possível excluir uma moto com locações associadas." });

            _context.Motos.Remove(moto);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("motos2024")]
        public async Task<ActionResult> GetMotos2024(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
                return BadRequest(new { mensagem = "Página e tamanho da página devem ser positivos." });

            var query = _context.Motos2024.AsQueryable();

            var totalItems = await query.CountAsync();
            var motos2024 = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                totalItems,
                page,
                pageSize,
                items = motos2024
            });
        }
    }
}
