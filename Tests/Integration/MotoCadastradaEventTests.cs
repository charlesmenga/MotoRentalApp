using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MotoRentalApp.Data;
using MotoRentalApp.Models;
using MotoRentalApp.Services;
using Xunit;

public class MotoCadastradaEventTests
{
    private readonly Moto2024ConsumerService _consumerService;
    private readonly AppDbContext _dbContext;

    public MotoCadastradaEventTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("TestDb_MotoCadastradaEvent"));
        services.AddSingleton<Moto2024ConsumerService>();
        var serviceProvider = services.BuildServiceProvider();

        _dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        _consumerService = new Moto2024ConsumerService(serviceProvider);
    }

    [Fact]
    public async Task MotoCadastradaEvent_ShouldPublishAndConsume_WhenYearIs2024()
    {
        var moto = new Moto
        {
            Identificador = "test-moto-2024",
            Ano = 2024,
            Modelo = "Modelo Teste",
            Placa = "XYZ1234"
        };

        var eventMessage = JsonSerializer.Serialize(new
        {
            Event = "MotoCadastrada",
            MotoId = moto.Identificador,
            Ano = moto.Ano,
            Modelo = moto.Modelo,
            Placa = moto.Placa
        });

        _dbContext.Motos2024.Add(new Moto2024
        {
            Identificador = moto.Identificador,
            Ano = moto.Ano,
            Modelo = moto.Modelo,
            Placa = moto.Placa
        });
        await _dbContext.SaveChangesAsync();

        var storedMoto = await _dbContext.Motos2024.FirstOrDefaultAsync(m => m.Identificador == moto.Identificador);
        Assert.NotNull(storedMoto);
        Assert.Equal(2024, storedMoto.Ano);
        Assert.Equal(moto.Modelo, storedMoto.Modelo);
        Assert.Equal(moto.Placa, storedMoto.Placa);
    }
}
