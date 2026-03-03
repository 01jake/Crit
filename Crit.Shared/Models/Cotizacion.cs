using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class Cotizacion
    {
        public int Id { get; set; }

        [Required]
        public string NumeroCotizacion { get; set; } = string.Empty;

        [Required]
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        public DateTime FechaVencimiento { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Descuento { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal IVA { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [StringLength(50)]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Aprobada, Rechazada, Convertida

        [StringLength(1000)]
        public string? Notas { get; set; }

        [StringLength(500)]
        public string? TerminosCondiciones { get; set; }

        public int? VentaId { get; set; } // Si se convierte en venta

        public string UsuarioId { get; set; } = string.Empty;

        // Relaciones
        public ICollection<DetalleCotizacion> Detalles { get; set; } = new List<DetalleCotizacion>();
    }
}
