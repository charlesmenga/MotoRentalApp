using Xunit;
using Microsoft.AspNetCore.Mvc;
using MotoRentalApp.Controllers;
using MotoRentalApp.Data;
using MotoRentalApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Moq;
using System.Collections.Generic;
using System.Linq;
using MotoRentalApp.Services;

namespace MotoRentalApp.Tests.UnitTests.Controllers
{
    public class MotosControllerTests
    {
        private readonly DbContextOptions<AppDbContext> _options;

        public MotosControllerTests()
        {
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("TestDb_Motos")
                .Options;
        }

        [Fact]
        public async Task CreateMoto_ShouldReturnBadRequest_WhenPlacaExists()
        {
            using var context = new AppDbContext(_options);
            context.Motos.Add(new Moto { Identificador = "m1", Placa = "XYZ1234", Modelo = "CG", Ano = 2022 });
            await context.SaveChangesAsync();

            var mockRabbit = new Mock<IRabbitMQService>();
            var controller = new MotosController(context, mockRabbit.Object);

            var moto = new Moto { Identificador = "m1", Placa = "XYZ1234", Modelo = "CG", Ano = 2022 };
            var result = await controller.CreateMoto(moto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
            Assert.Contains("Placa já cadastrada", badRequest.Value!.ToString());
        }

        [Fact]
        public async Task CreateMoto_ShouldReturnCreated_WhenValid()
        {
            using var context = new AppDbContext(_options);
            var mockRabbit = new Mock<IRabbitMQService>();
            mockRabbit.Setup(r => r.SendMessage(It.IsAny<string>())).Verifiable();

            var controller = new MotosController(context, mockRabbit.Object);

            var moto = new Moto { Identificador = "m2", Placa = "ABC1234", Modelo = "CB500", Ano = 2023 };
            var result = await controller.CreateMoto(moto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var createdMoto = Assert.IsType<Moto>(createdResult.Value);
            Assert.Equal("ABC1234", createdMoto.Placa);

            mockRabbit.Verify(r => r.SendMessage(It.Is<string>(msg => msg.Contains("ABC1234"))), Times.Once);
        }

        [Fact]
        public async Task GetMotos_ShouldReturnAllMotos()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_Motos_GetAll")
                .Options;
            using var context = new AppDbContext(options);
            context.Motos.AddRange(new List<Moto>
            {
                new Moto { Identificador = "m3", Placa = "AAA1111", Modelo = "XRE", Ano = 2021 },
                new Moto { Identificador = "m4", Placa = "BBB2222", Modelo = "CB500", Ano = 2022 }
            });
            await context.SaveChangesAsync();

            var mockRabbit = new Mock<IRabbitMQService>();
            var controller = new MotosController(context, mockRabbit.Object);

            var result = await controller.GetMotos(null);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var anon = okResult.Value;
            var motos = (IEnumerable<Moto>)anon.GetType().GetProperty("items").GetValue(anon, null);
            Assert.Equal(2, motos.Count());
        }

        [Fact]
        public async Task GetMotos_ShouldFilterByPlaca()
        {
            using var context = new AppDbContext(_options);
            context.Motos.AddRange(new List<Moto>
            {
                new Moto { Identificador = "m5", Placa = "CCC3333", Modelo = "XRE", Ano = 2021 },
                new Moto { Identificador = "m6", Placa = "DDD4444", Modelo = "CB500", Ano = 2022 }
            });
            await context.SaveChangesAsync();

            var mockRabbit = new Mock<IRabbitMQService>();
            var controller = new MotosController(context, mockRabbit.Object);

            var result = await controller.GetMotos("CCC3333");
            var okResult = Assert.IsType<OkObjectResult>(result);
            var anon = okResult.Value;
            var motos = (IEnumerable<Moto>)anon.GetType().GetProperty("items").GetValue(anon, null);
            Assert.Single(motos);
            Assert.Equal("CCC3333", motos.First().Placa);
        }

        [Fact]
        public async Task UpdatePlaca_ShouldChangePlaca()
        {
            using var context = new AppDbContext(_options);
            context.Motos.Add(new Moto { Identificador = "m7", Placa = "EEE5555", Modelo = "XRE", Ano = 2021 });
            await context.SaveChangesAsync();

            var mockRabbit = new Mock<IRabbitMQService>();
            var controller = new MotosController(context, mockRabbit.Object);

            var motoUpdate = new Moto { Identificador = "m7", Placa = "FFF6666", Modelo = "XRE", Ano = 2021 };
            var result = await controller.UpdatePlaca("m7", motoUpdate);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedMoto = await context.Motos.FindAsync("m7");
            Assert.Equal("FFF6666", updatedMoto.Placa);
        }

        [Fact]
        public async Task DeleteMoto_ShouldReturnBadRequest_WhenHasLocacoes()
        {
            using var context = new AppDbContext(_options);
            context.Motos.Add(new Moto { Identificador = "m8", Placa = "GGG7777", Modelo = "XRE", Ano = 2021 });
            context.Locacoes.Add(new Locacao { Identificador = "l1", MotoId = "m8", EntregadorId = "e1" });
            await context.SaveChangesAsync();

            var mockRabbit = new Mock<IRabbitMQService>();
            var controller = new MotosController(context, mockRabbit.Object);

            var result = await controller.DeleteMoto("m8");

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Não é possível excluir uma moto com locações associadas", badRequest.Value.ToString());
        }

        [Fact]
        public async Task DeleteMoto_ShouldReturnNoContent_WhenNoLocacoes()
        {
            using var context = new AppDbContext(_options);
            context.Motos.Add(new Moto { Identificador = "m9", Placa = "HHH8888", Modelo = "XRE", Ano = 2021 });
            await context.SaveChangesAsync();

            var mockRabbit = new Mock<IRabbitMQService>();
            var controller = new MotosController(context, mockRabbit.Object);

            var result = await controller.DeleteMoto("m9");

            Assert.IsType<OkResult>(result);
        }
    }
}
