using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRentalApp.Data;
using MotoRentalApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MotoRentalApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EntregadoresController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly string _cnhImageFolder = Path.Combine(Directory.GetCurrentDirectory(), "CNHImages");

        public EntregadoresController(AppDbContext context)
        {
            _context = context;
            if (!Directory.Exists(_cnhImageFolder))
            {
                Directory.CreateDirectory(_cnhImageFolder);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetEntregadores(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
                return BadRequest(new { mensagem = "Página e tamanho da página devem ser positivos." });

            var totalItems = await _context.Entregadores.CountAsync();
            var entregadores = await _context.Entregadores
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                totalItems,
                page,
                pageSize,
                items = entregadores
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateEntregador([FromBody] Entregador entregador)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { mensagem = "Dados inválidos" });
            }

            var validTipos = new[] { "A", "B", "A+B" };
            if (string.IsNullOrEmpty(entregador.TipoCnh) || !validTipos.Contains(entregador.TipoCnh))
            {
                return BadRequest(new { mensagem = "Tipo de CNH inválido" });
            }

            var existingCnpj = await _context.Entregadores.AnyAsync(e => e.Cnpj == entregador.Cnpj);
            if (existingCnpj)
            {
                return BadRequest(new { mensagem = "CNPJ já cadastrado" });
            }

            var existingCnh = await _context.Entregadores.AnyAsync(e => e.NumeroCnh == entregador.NumeroCnh);
            if (existingCnh)
            {
                return BadRequest(new { mensagem = "Número da CNH já cadastrado" });
            }

            entregador.ImagemCnhPath = null;

            _context.Entregadores.Add(entregador);
            await _context.SaveChangesAsync();

            return StatusCode(201, entregador);
        }

        [HttpPost("{id}/cnh")]
        public async Task<IActionResult> UploadCnhImage(string id, [FromBody] CnhImageUploadRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ImagemCnh))
            {
                return BadRequest(new { mensagem = "Dados inválidos" });
            }

            var entregador = await _context.Entregadores.FindAsync(id);
            if (entregador == null)
            {
                return BadRequest(new { mensagem = "Entregador não encontrado" });
            }

            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(request.ImagemCnh);
            }
            catch
            {
                return BadRequest(new { mensagem = "Imagem CNH inválida" });
            }

            if (!IsPngOrBmp(imageBytes))
            {
                return BadRequest(new { mensagem = "Formato de arquivo inválido. Apenas PNG ou BMP são permitidos." });
            }

            var fileName = $"{id}_{Guid.NewGuid()}.png";
            var filePath = Path.Combine(_cnhImageFolder, fileName);
            await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

            entregador.ImagemCnhPath = filePath;
            await _context.SaveChangesAsync();

            return StatusCode(201);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEntregadorById(string id)
        {
            var entregador = await _context.Entregadores.FindAsync(id);
            if (entregador == null)
            {
                return NotFound(new { mensagem = "Entregador não encontrado" });
            }
            return Ok(entregador);
        }

        private bool IsPngOrBmp(byte[] bytes)
        {
            byte[] pngHeader = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
            byte[] bmpHeader = new byte[] { 66, 77 };

            if (bytes.Length >= 8 && bytes.Take(8).SequenceEqual(pngHeader))
                return true;
            if (bytes.Length >= 2 && bytes.Take(2).SequenceEqual(bmpHeader))
                return true;
            return false;
        }
    }

    public class CnhImageUploadRequest
    {
        public string? ImagemCnh { get; set; }
    }
}
