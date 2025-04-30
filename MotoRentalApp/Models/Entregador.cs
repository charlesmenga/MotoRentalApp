using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MotoRentalApp.Models
{
    [PrimaryKey(nameof(Identificador))]
    public class Entregador
    {
        public required string Identificador { get; set; }

        [Required]
        public required string Nome { get; set; }

        [Required]
        [StringLength(14)]
        public required string Cnpj { get; set; }

        [Required]
        public DateTime DataNascimento { get; set; }

        [Required]
        [StringLength(11)]
        public required string NumeroCnh { get; set; }

        [Required]
        [RegularExpression("A|B|A\\+B")]
        public required string TipoCnh { get; set; }

        public string? ImagemCnhPath { get; set; }
    }
}
