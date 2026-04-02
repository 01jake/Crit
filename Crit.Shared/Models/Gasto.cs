using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class Gasto
    {
        public int Id { get; set; }
        public int? EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        [StringLength(100)]
        public string Concepto { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Categoria { get; set; }
        // Renta, Nómina, Transporte, Servicios, Papelería, Viáticos, Mantenimiento, Otros

        public decimal Monto { get; set; }

        [StringLength(50)]
        public string? MetodoPago { get; set; }

        [StringLength(100)]
        public string? Referencia { get; set; }

        [StringLength(250)]
        public string? Observaciones { get; set; }

        public int? ProveedorId { get; set; }
        public Proveedor? Proveedor { get; set; }

        public int? CajaSesionId { get; set; }
        public CajaSesion? CajaSesion { get; set; }

        public int? CajaMovimientoId { get; set; }
        public CajaMovimiento? CajaMovimiento { get; set; }

        public string? UsuarioId { get; set; }

        public bool Activo { get; set; } = true;
    }
}
