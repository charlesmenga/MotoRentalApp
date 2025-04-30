using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MotoRentalApp.Models
{
    [PrimaryKey(nameof(Identificador))]
    public class Moto
    {
        [Key]
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
