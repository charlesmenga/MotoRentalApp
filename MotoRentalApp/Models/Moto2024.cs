using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MotoRentalApp.Models
{
    [Table("Motos2024")]
    [PrimaryKey(nameof(Identificador))]
    public class Moto2024
    {
        public required string Identificador { get; set; }

        [Required]
        public int Ano { get; set; }

        [Required]
        public required string Modelo { get; set; }

        [Required]
        [StringLength(10)]
        public required string Placa { get; set; }
    }
}
