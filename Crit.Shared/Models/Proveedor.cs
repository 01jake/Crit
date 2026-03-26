using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class Proveedor
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200)]
        public string? RazonSocial { get; set; }

        [Required]
        [StringLength(13)]
        public string RFC { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Telefono { get; set; }

        [StringLength(250)]
        public string? Direccion { get; set; }

        [StringLength(100)]
        public string? Contacto { get; set; }

        [StringLength(50)]
        public string? RegimenFiscal { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public bool Activo { get; set; } = true;
        public ICollection<CuentaPorPagar>? CuentasPorPagar { get; set; }

    }
}
