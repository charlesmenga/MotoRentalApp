using Xunit;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MotoRentalApp.Tests.IntegrationTests.Controllers
{
    public class EntregadoresControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public EntregadoresControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetEntregadores_ShouldReturnOk()
        {
            var response = await _client.GetAsync("/Entregadores");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CreateEntregador_ShouldReturnCreated()
        {
            var entregador = new
            {
                Identificador = "Entregador John",
                Nome = "John",
                Cnpj = "00011122233344",
                NumeroCnh = "1234567890",
                TipoCnh = "A"
            };

            var content = new StringContent(JsonSerializer.Serialize(entregador), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/Entregadores", content);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
    }
}
