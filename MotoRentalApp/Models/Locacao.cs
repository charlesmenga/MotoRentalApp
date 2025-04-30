using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MotoRentalApp.Models
{
    [PrimaryKey(nameof(Identificador))]
    public class Locacao
    {
        public required string Identificador { get; set; }

        [Required]
        public required string EntregadorId { get; set; }

        [Required]
        public required string MotoId { get; set; }

        [Required]
        public DateTime DataInicio { get; set; }

        [Required]
        public DateTime DataTermino { get; set; }

        [Required]
        public DateTime DataPrevisaoTermino { get; set; }

        public DateTime? DataDevolucao { get; set; }

        [Required]
        public int Plano { get; set; }

        public decimal ValorDiaria { get; set; }

        public decimal ValorTotal { get; set; }
    }
}
