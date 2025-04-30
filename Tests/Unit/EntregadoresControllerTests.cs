using System;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using MotoRentalApp.Controllers;
using MotoRentalApp.Data;
using MotoRentalApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Text;

namespace MotoRentalApp.Tests.UnitTests.Controllers
{
    public class EntregadoresControllerTests
    {
        private AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task CreateEntregador_ShouldReturn201_WhenValid()
        {
            using var context = CreateContext("Db_CreateValid");
            var controller = new EntregadoresController(context);
            var entregador = new Entregador
            {
                Identificador = "e1",
                Nome = "John",
                Cnpj = "12345678901234",
                NumeroCnh = "CNH123456",
                TipoCnh = "A"
            };

            var result = await controller.CreateEntregador(entregador);

            var created = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, created.StatusCode);
        }

        [Fact]
        public async Task GetEntregadores_ShouldReturnPaginatedList()
        {
            using var context = CreateContext("Db_GetEntregadores");
            context.Entregadores.Add(new Entregador
            {
                Identificador = "e1",
                Nome = "Teste",
                Cnpj = "11111111111111",
                NumeroCnh = "2222222222",
                TipoCnh = "A+B"
            });
            await context.SaveChangesAsync();

            var controller = new EntregadoresController(context);
            var result = await controller.GetEntregadores(1, 10);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task CreateEntregador_ShouldReturnBadRequest_WhenCnpjExists()
        {
            using var context = CreateContext("Db_CnpjExists");
            context.Entregadores.Add(new Entregador { Identificador = "e1", Nome = "Test", Cnpj = "123", NumeroCnh = "CNH1", TipoCnh = "A" });
            await context.SaveChangesAsync();

            var controller = new EntregadoresController(context);
            var entregador = new Entregador { Identificador = "e2", Nome = "Test2", Cnpj = "123", NumeroCnh = "CNH2", TipoCnh = "A" };

            var result = await controller.CreateEntregador(entregador);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("CNPJ já cadastrado", badRequest.Value.ToString());
        }

        [Fact]
        public async Task CreateEntregador_ShouldReturnBadRequest_WhenNumeroCnhExists()
        {
            using var context = CreateContext("Db_CnhExists");
            context.Entregadores.Add(new Entregador { Identificador = "e1", Nome = "Test", Cnpj = "111", NumeroCnh = "CNH999", TipoCnh = "A" });
            await context.SaveChangesAsync();

            var controller = new EntregadoresController(context);
            var entregador = new Entregador { Identificador = "e2", Nome = "Test2", Cnpj = "222", NumeroCnh = "CNH999", TipoCnh = "B" };

            var result = await controller.CreateEntregador(entregador);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Número da CNH já cadastrado", badRequest.Value.ToString());
        }

        [Theory]
        [InlineData("A")]
        [InlineData("B")]
        [InlineData("A+B")]
        public async Task CreateEntregador_ShouldAcceptValidTipoCnh(string tipoCnh)
        {
            using var context = CreateContext($"Db_ValidTipoCnh_{tipoCnh}");
            var controller = new EntregadoresController(context);
            var entregador = new Entregador
            {
                Identificador = Guid.NewGuid().ToString(),
                Nome = "Valido",
                Cnpj = Guid.NewGuid().ToString().Substring(0, 14).PadLeft(14, '0'),
                NumeroCnh = Guid.NewGuid().ToString().Substring(0, 10).PadLeft(10, '0'),
                TipoCnh = tipoCnh
            };

            var result = await controller.CreateEntregador(entregador);

            var created = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, created.StatusCode);
        }

        [Theory]
        [InlineData("C")]
        [InlineData("D")]
        [InlineData("")]
        [InlineData(null)]
        public async Task CreateEntregador_ShouldReturnBadRequest_WhenInvalidTipoCnh(string tipoCnh)
        {
            using var context = CreateContext($"Db_InvalidTipoCnh_{tipoCnh ?? "null"}");
            var controller = new EntregadoresController(context);
            var entregador = new Entregador
            {
                Identificador = "inv",
                Nome = "Invalido",
                Cnpj = "55555555555555",
                NumeroCnh = "5555555555",
                TipoCnh = tipoCnh
            };

            var result = await controller.CreateEntregador(entregador);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Tipo de CNH inválido", badRequest.Value.ToString());
        }

        [Fact]
        public async Task UploadCnhImage_ShouldReturnBadRequest_WhenRequestIsNull()
        {
            using var context = CreateContext("Db_NullRequest");
            var controller = new EntregadoresController(context);

            var result = await controller.UploadCnhImage("any-id", null);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Dados inválidos", badRequest.Value.ToString());
        }

        [Fact]
        public async Task UploadCnhImage_ShouldReturnBadRequest_WhenEntregadorNotFound()
        {
            using var context = CreateContext("Db_NotFoundEntregador");
            var controller = new EntregadoresController(context);

            var request = new CnhImageUploadRequest { ImagemCnh = Convert.ToBase64String(Encoding.UTF8.GetBytes("fake")) };
            var result = await controller.UploadCnhImage("missing-id", request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Entregador não encontrado", badRequest.Value.ToString());
        }

        [Fact]
        public async Task UploadCnhImage_ShouldReturnBadRequest_WhenImageIsInvalidBase64()
        {
            using var context = CreateContext("Db_InvalidBase64");
            var entregador = new Entregador { Identificador = "e1", Nome = "Test", Cnpj = "111", NumeroCnh = "999", TipoCnh = "A" };
            context.Entregadores.Add(entregador);
            await context.SaveChangesAsync();

            var controller = new EntregadoresController(context);
            var request = new CnhImageUploadRequest { ImagemCnh = "not-base64" };

            var result = await controller.UploadCnhImage("e1", request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Imagem CNH inválida", badRequest.Value.ToString());
        }

        [Fact]
        public async Task UploadCnhImage_ShouldReturnBadRequest_WhenImageFormatIsInvalid()
        {
            using var context = CreateContext("Db_InvalidFormat");
            var entregador = new Entregador { Identificador = "e2", Nome = "Test", Cnpj = "222", NumeroCnh = "888", TipoCnh = "B" };
            context.Entregadores.Add(entregador);
            await context.SaveChangesAsync();

            var controller = new EntregadoresController(context);
            var jpegHeader = new byte[] { 255, 216, 255, 224 };
            var request = new CnhImageUploadRequest { ImagemCnh = Convert.ToBase64String(jpegHeader) };

            var result = await controller.UploadCnhImage("e2", request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Formato de arquivo inválido", badRequest.Value.ToString());
        }

        [Fact]
        public async Task UploadCnhImage_ShouldReturn201_WhenValidPng()
        {
            using var context = CreateContext("Db_ValidPng");
            var entregador = new Entregador { Identificador = "e3", Nome = "PngTest", Cnpj = "333", NumeroCnh = "777", TipoCnh = "A" };
            context.Entregadores.Add(entregador);
            await context.SaveChangesAsync();

            var controller = new EntregadoresController(context);
            var pngHeader = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
            var request = new CnhImageUploadRequest { ImagemCnh = Convert.ToBase64String(pngHeader) };

            var result = await controller.UploadCnhImage("e3", request);

            var created = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(201, created.StatusCode);
        }

        [Fact]
        public async Task UploadCnhImage_ShouldReturn201_WhenValidBmp()
        {
            using var context = CreateContext("Db_ValidBmp");
            var entregador = new Entregador { Identificador = "e4", Nome = "BmpTest", Cnpj = "444", NumeroCnh = "666", TipoCnh = "B" };
            context.Entregadores.Add(entregador);
            await context.SaveChangesAsync();

            var controller = new EntregadoresController(context);
            var bmpHeader = new byte[] { 66, 77 };
            var request = new CnhImageUploadRequest { ImagemCnh = Convert.ToBase64String(bmpHeader) };

            var result = await controller.UploadCnhImage("e4", request);

            var created = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(201, created.StatusCode);
        }
    }
}
