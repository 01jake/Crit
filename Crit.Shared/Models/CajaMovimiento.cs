using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class CajaMovimiento
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }

        [Required]
        public int CajaSesionId { get; set; }
        public CajaSesion? CajaSesion { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string Tipo { get; set; } = "Ingreso";
        // Ingreso, Egreso, Ajuste

        [StringLength(30)]
        public string Origen { get; set; } = "Manual";
        // Apertura, VentaContado, AbonoCliente, PagoProveedor, Gasto, Retiro, Ajuste, Manual

        public decimal Monto { get; set; }

        public decimal SaldoAnterior { get; set; }
        public decimal SaldoPosterior { get; set; }

        public int? VentaId { get; set; }
        public int? CuentaPorCobrarId { get; set; }
        public int? CuentaPorPagarId { get; set; }
        public int? GastoId { get; set; }

        [StringLength(100)]
        public string? Referencia { get; set; }

        [StringLength(250)]
        public string? Concepto { get; set; }

        [StringLength(50)]
        public string? MetodoPago { get; set; }

        public string? UsuarioId { get; set; }

        public bool Activo { get; set; } = true;

    }
}
