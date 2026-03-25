using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class Venta
    {
        public int Id { get; set; }
        public string? NumeroVenta { get; set; }  // ← Nullable, el servidor lo genera
        [Required]
        public int ClienteId { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal IVA { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = "Completada";
        public string? Notas { get; set; }
        public string? UsuarioId { get; set; }
        public string? MetodoPago { get; set; } 
        public string? FormaPago { get; set; }  
        public string? UsoCFDI { get; set; }
        // Navegación
        public Cliente? Cliente { get; set; }
        public List<DetalleVenta> Detalles { get; set; } = new();
    }
}
