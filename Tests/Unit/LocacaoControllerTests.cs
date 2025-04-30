using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRentalApp.Controllers;
using MotoRentalApp.Data;
using MotoRentalApp.Models;
using System;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json.Linq;

namespace MotoRentalApp.Tests.UnitTests.Controllers
{
    public class LocacaoControllerTests
    {
        private async Task<AppDbContext> GetInMemoryDbContextAsync()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);

            var entregador = new Entregador
            {
                Identificador = "Entregador João",
                Nome = "João",
                TipoCnh = "A",
                Cnpj = "12345678901234",
                NumeroCnh = "123456789",
            };

            var moto = new Moto
            {
                Identificador = "Moto Honda CG",
                Modelo = "Honda CG",
                Placa = "ABC-1234"
            };

            var locacao = new Locacao
            {
                Identificador = "loc1",
                EntregadorId = "Entregador João",
                MotoId = "Moto Honda CG",
                DataInicio = DateTime.UtcNow.Date,
                DataPrevisaoTermino = DateTime.UtcNow.Date.AddDays(7),
                Plano = 7,
                ValorDiaria = 30m
            };

            context.Entregadores.Add(entregador);
            context.Motos.Add(moto);
            context.Locacoes.Add(locacao);

            await context.SaveChangesAsync();
            return context;
        }

        [Fact]
        public async Task UpdateDevolucao_ShouldReturnCorrectAmount_WhenReturnedEarly()
        {
            var context = await GetInMemoryDbContextAsync();
            var controller = new LocacaoController(context);

            var request = new DevolucaoRequest
            {
                DataDevolucao = DateTime.UtcNow.Date.AddDays(4)
            };

            var actionResult = await controller.UpdateDevolucao("loc1", request);

            var result = Assert.IsType<OkObjectResult>(actionResult);
            var json = JObject.FromObject(result.Value);

            Assert.Equal("Data de devolução informada com sucesso", json["mensagem"].ToString());
            decimal expectedTotal = (4 * 30m) + (3 * 30m * 0.2m);
            Assert.Equal(expectedTotal, json["valorTotal"].ToObject<decimal>());
        }

        [Fact]
        public async Task UpdateDevolucao_ShouldReturnCorrectAmount_WhenReturnedLate()
        {
            var context = await GetInMemoryDbContextAsync();
            var controller = new LocacaoController(context);

            var request = new DevolucaoRequest
            {
                DataDevolucao = DateTime.UtcNow.Date.AddDays(10)
            };

            var actionResult = await controller.UpdateDevolucao("loc1", request);

            var result = Assert.IsType<OkObjectResult>(actionResult);
            var json = JObject.FromObject(result.Value);

            Assert.Equal("Data de devolução informada com sucesso", json["mensagem"].ToString());
            decimal expectedTotal = (7 * 30m) + (3 * 50m);
            Assert.Equal(expectedTotal, json["valorTotal"].ToObject<decimal>());
        }

         [Fact]
        public async Task UpdateDevolucao_ShouldReturnCorrectAmount_WhenReturnedExactlyOnTime()
        {
            var context = await GetInMemoryDbContextAsync();
            var controller = new LocacaoController(context);

            var request = new DevolucaoRequest
            {
                DataDevolucao = DateTime.UtcNow.Date.AddDays(7)
            };

            var actionResult = await controller.UpdateDevolucao("loc1", request);

            var result = Assert.IsType<OkObjectResult>(actionResult);
            var json = JObject.FromObject(result.Value);

            Assert.Equal("Data de devolução informada com sucesso", json["mensagem"].ToString());
            decimal expectedTotal = 7 * 30m;
            Assert.Equal(expectedTotal, json["valorTotal"].ToObject<decimal>());
        }

        [Fact]
        public async Task UpdateDevolucao_ShouldReturnMinimumCharge_WhenReturnedSameDay()
        {
            var context = await GetInMemoryDbContextAsync();
            var controller = new LocacaoController(context);

            var request = new DevolucaoRequest
            {
                DataDevolucao = DateTime.UtcNow.Date
            };

            var actionResult = await controller.UpdateDevolucao("loc1", request);

            var result = Assert.IsType<OkObjectResult>(actionResult);
            var json = JObject.FromObject(result.Value);

            Assert.Equal("Data de devolução informada com sucesso", json["mensagem"].ToString());
            decimal expectedTotal = 1 * 30m;
            Assert.Equal(expectedTotal, json["valorTotal"].ToObject<decimal>());
        }

        [Theory]
        [InlineData(7, 30)]
        [InlineData(15, 28)]
        [InlineData(30, 22)]
        [InlineData(45, 20)]
        [InlineData(50, 18)]
        public async Task CreateLocacao_ShouldReturn201_WhenValidPlan(int plano, decimal valorDiaria)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_Locacao_ValidPlan_{plano}")
                .Options;

            using var context = new AppDbContext(options);

            var entregador = new Entregador
            {
                Identificador = "Entregador Test",
                Nome = "Test",
                TipoCnh = "A",
                Cnpj = "12345678901234",
                NumeroCnh = "123456789"
            };

            var moto = new Moto
            {
                Identificador = "Moto Test",
                Modelo = "Modelo Test",
                Placa = "TEST1234"
            };

            context.Entregadores.Add(entregador);
            context.Motos.Add(moto);
            await context.SaveChangesAsync();

            var controller = new LocacaoController(context);

            var locacao = new Locacao
            {
                Identificador = Guid.NewGuid().ToString(),
                EntregadorId = entregador.Identificador,
                MotoId = moto.Identificador,
                DataInicio = DateTime.UtcNow.Date.AddDays(1),
                DataTermino = DateTime.UtcNow.Date.AddDays(1 + plano),
                DataPrevisaoTermino = DateTime.UtcNow.Date.AddDays(1 + plano),
                Plano = plano
            };

            var result = await controller.CreateLocacao(locacao);

            var createdResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, createdResult.StatusCode);

            var createdLocacao = Assert.IsType<Locacao>(createdResult.Value);
            Assert.Equal(valorDiaria, createdLocacao.ValorDiaria);
            Assert.Equal(plano, createdLocacao.Plano);
        }
    }
}
