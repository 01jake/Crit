using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class CuentaPorCobrar
    {
        public int Id { get; set; }
        public int? EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }


        [Required]
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public int? VentaId { get; set; }
        public Venta? Venta { get; set; }

        [StringLength(50)]
        public string? Folio { get; set; }

        public DateTime FechaEmision { get; set; } = DateTime.Now;
        public DateTime? FechaVencimiento { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal IVA { get; set; }
        public decimal Total { get; set; }

        public decimal TotalPagado { get; set; } = 0m;

        public decimal Saldo => Total - TotalPagado;

        public bool EstaVencida =>
            Saldo > 0 &&
            FechaVencimiento.HasValue &&
            FechaVencimiento.Value.Date < DateTime.Today;

        public int DiasVencidos =>
            EstaVencida
                ? (DateTime.Today - FechaVencimiento!.Value.Date).Days
                : 0;

        [StringLength(30)]
        public string Estado { get; set; } = "Pendiente";
        // Pendiente, Parcial, Pagada, Vencida, Cancelada

        [StringLength(250)]
        public string? Observaciones { get; set; }

        public DateTime? FechaUltimoPago { get; set; }

        public bool Activa { get; set; } = true;

        public ICollection<PagoCliente> Pagos { get; set; } = new List<PagoCliente>();
    }

}
