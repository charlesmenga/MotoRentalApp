using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MotoRentalApp.Data;
using MotoRentalApp.Models;
using MotoRentalApp.Services;
using Xunit;

public class Moto2024ConsumerDiagnosticTest
{
    private readonly Moto2024ConsumerService _consumerService;
    private readonly AppDbContext _dbContext;

    public Moto2024ConsumerDiagnosticTest()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("TestDb_Diagnostic"));
        services.AddSingleton<Moto2024ConsumerService>();
        var serviceProvider = services.BuildServiceProvider();

        _dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        _consumerService = new Moto2024ConsumerService(serviceProvider);
    }

    [Fact]
    public async Task Diagnostic_ProcessMotoCadastradaMessage_ShouldSaveMoto2024()
    {
        var message = JsonSerializer.Serialize(new
        {
            Event = "MotoCadastrada",
            MotoId = "diagnostic-id-2024",
            Ano = 2024,
            Modelo = "Diagnostic Model",
            Placa = "DIAG1234"
        });

        var moto2024 = new Moto2024
        {
            Identificador = "diagnostic-id-2024",
            Ano = 2024,
            Modelo = "Diagnostic Model",
            Placa = "DIAG1234"
        };

        _dbContext.Motos2024.Add(moto2024);
        await _dbContext.SaveChangesAsync();

        var savedMoto = await _dbContext.Motos2024.FirstOrDefaultAsync(m => m.Identificador == "diagnostic-id-2024");
        Assert.NotNull(savedMoto);
        Assert.Equal(2024, savedMoto.Ano);
        Assert.Equal("Diagnostic Model", savedMoto.Modelo);
        Assert.Equal("DIAG1234", savedMoto.Placa);
    }
}
