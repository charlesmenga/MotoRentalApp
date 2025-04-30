using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRentalApp.Data;
using MotoRentalApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MotoRentalApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LocacaoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LocacaoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetLocacoes(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
                return BadRequest(new { mensagem = "Página e tamanho da página devem ser positivos." });

            var totalItems = await _context.Locacoes.CountAsync();
            var locacoes = await _context.Locacoes
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                totalItems,
                page,
                pageSize,
                items = locacoes
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateLocacao([FromBody] Locacao locacao)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { mensagem = "Dados inválidos" });
            }

            var entregador = await _context.Entregadores.FindAsync(locacao.EntregadorId);
            if (entregador == null || (entregador.TipoCnh != "A" && entregador.TipoCnh != "A+B"))
            {
                return BadRequest(new { mensagem = "Entregador não habilitado para locação" });
            }

            var moto = await _context.Motos.FindAsync(locacao.MotoId);
            if (moto == null)
            {
                return BadRequest(new { mensagem = "Moto não encontrada" });
            }

            if (locacao.DataInicio.Date != DateTime.UtcNow.Date.AddDays(1))
            {
                return BadRequest(new { mensagem = "Data de início inválida" });
            }
            if (locacao.DataTermino <= locacao.DataInicio || locacao.DataPrevisaoTermino <= locacao.DataInicio)
            {
                return BadRequest(new { mensagem = "Datas inválidas" });
            }

            int[] planosValidos = { 7, 15, 30, 45, 50 };
            if (Array.IndexOf(planosValidos, locacao.Plano) < 0)
            {
                return BadRequest(new { mensagem = "Plano inválido" });
            }

            decimal valorDiaria = locacao.Plano switch
            {
                7 => 30m,
                15 => 28m,
                30 => 22m,
                45 => 20m,
                50 => 18m,
                _ => 0m
            };
            locacao.ValorDiaria = valorDiaria;

            locacao.Identificador = Guid.NewGuid().ToString();

            _context.Locacoes.Add(locacao);
            await _context.SaveChangesAsync();

            return StatusCode(201, locacao);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLocacaoById(string id)
        {
            var locacao = await _context.Locacoes.FindAsync(id);
            if (locacao == null)
            {
                return NotFound(new { mensagem = "Locação não encontrada" });
            }
            return Ok(locacao);
        }

        [HttpPut("{id}/devolucao")]
        public async Task<IActionResult> UpdateDevolucao(string id, [FromBody] DevolucaoRequest request)
        {
            if (!ModelState.IsValid || request == null || request.DataDevolucao == default)
            {
                return BadRequest(new { mensagem = "Dados inválidos" });
            }

            var locacao = await _context.Locacoes.FindAsync(id);
            if (locacao == null)
            {
                return NotFound(new { mensagem = "Locação não encontrada" });
            }

            locacao.DataDevolucao = request.DataDevolucao;

            decimal total = 0m;
            int diasLocados = (locacao.DataPrevisaoTermino - locacao.DataInicio).Days;
            int diasUsados = (locacao.DataDevolucao.Value.Date - locacao.DataInicio.Date).Days;

            decimal valorDiaria = locacao.ValorDiaria;

            if (locacao.DataDevolucao < locacao.DataPrevisaoTermino)
            {
                int diasNaoUsados = diasLocados - diasUsados;
                decimal multaPercentual = 0m;

                if (locacao.Plano == 7)
                    multaPercentual = 0.20m;
                else if (locacao.Plano == 15)
                    multaPercentual = 0.40m;

                total = (valorDiaria * diasUsados) + (valorDiaria * diasNaoUsados * multaPercentual);
            }
            else if (locacao.DataDevolucao > locacao.DataPrevisaoTermino)
            {
                int diasAdicionais = (locacao.DataDevolucao.Value.Date - locacao.DataPrevisaoTermino.Date).Days;
                total = (valorDiaria * diasLocados) + (diasAdicionais * 50m);
            }
            else
            {
                total = valorDiaria * diasLocados;
            }

            if (locacao.DataDevolucao == locacao.DataInicio.Date)
            {
                total = valorDiaria;
            }

            locacao.ValorTotal = total;

            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Data de devolução informada com sucesso", valorTotal = total });
        }
    }

    public class DevolucaoRequest
    {
        public DateTime DataDevolucao { get; set; }
    }
}
