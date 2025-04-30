using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using MotoRentalApp.Data;
using System.Linq;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptors = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                d.ServiceType == typeof(AppDbContext) ||
                d.ImplementationType == typeof(AppDbContext) ||
                (d.ImplementationType != null && d.ImplementationType.FullName != null && 
                 (d.ImplementationType.FullName.Contains("Npgsql") || d.ImplementationType.FullName.Contains("PostgreSQL") || d.ImplementationType.FullName.Contains("EntityFrameworkCore"))) ||
                (d.ServiceType != null && d.ServiceType.FullName != null &&
                 (d.ServiceType.FullName.Contains("Npgsql") || d.ServiceType.FullName.Contains("PostgreSQL") || d.ServiceType.FullName.Contains("EntityFrameworkCore")))
                ).ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryTestDb");
            });
        });

        builder.ConfigureServices(services =>
        {
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (!db.Entregadores.Any())
            {
                db.Entregadores.Add(new MotoRentalApp.Models.Entregador
                {
                    Identificador = "Entregador Test",
                    Nome = "Test",
                    Cnpj = "12345678901234",
                    NumeroCnh = "9876543210",
                    TipoCnh = "A"
                });
                db.SaveChanges();
            }
        });
    }
}
