using Microsoft.EntityFrameworkCore;
using MotoRentalApp.Models;

namespace MotoRentalApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Moto> Motos { get; set; }
        public DbSet<Moto2024> Motos2024 { get; set; }

        public DbSet<Entregador> Entregadores { get; set; }
        public DbSet<Locacao> Locacoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Moto>()
                .HasIndex(m => m.Placa)
                .IsUnique();

            modelBuilder.Entity<Moto2024>()
                .HasIndex(m => m.Placa)
                .IsUnique();

            modelBuilder.Entity<Entregador>()
                .HasIndex(e => e.Cnpj)
                .IsUnique();

            modelBuilder.Entity<Entregador>()
                .HasIndex(e => e.NumeroCnh)
                .IsUnique();
        }
    }
}
