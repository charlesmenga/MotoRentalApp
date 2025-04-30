using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MotoRentalApp.Data;
using MotoRentalApp.Models;
using MotoRentalApp.Services;
using Xunit;

public class Moto2024ConsumerTests
{
    private readonly Moto2024ConsumerService _consumerService;
    private readonly AppDbContext _dbContext;

    public Moto2024ConsumerTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));
        services.AddSingleton<Moto2024ConsumerService>();
        var serviceProvider = services.BuildServiceProvider();

        _dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        _consumerService = new Moto2024ConsumerService(serviceProvider);
    }

    [Fact]
    public async Task Should_Save_Moto2024_When_Receive_MotoCadastrada_Event_With_Year_2024()
    {
        var message = JsonSerializer.Serialize(new
        {
            Event = "MotoCadastrada",
            MotoId = "test-id-2024",
            Ano = 2024,
            Modelo = "Modelo Teste",
            Placa = "ABC1234"
        });

        _dbContext.Motos2024.Add(new Moto2024
        {
            Identificador = "test-id-2024",
            Ano = 2024,
            Modelo = "Modelo Teste",
            Placa = "ABC1234"
        });
        await _dbContext.SaveChangesAsync();

        var moto = await _dbContext.Motos2024.FirstOrDefaultAsync(m => m.Identificador == "test-id-2024");
        Assert.NotNull(moto);
        Assert.Equal(2024, moto.Ano);
        Assert.Equal("Modelo Teste", moto.Modelo);
        Assert.Equal("ABC1234", moto.Placa);
    }
}
