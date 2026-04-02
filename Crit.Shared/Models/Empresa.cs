using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class Empresa
    {
        public int Id { get; set; }


        [Required]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(20)]
        public string? RFC { get; set; }

        [StringLength(100)]
        public string? Dominio { get; set; }

        public bool Activa { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}
