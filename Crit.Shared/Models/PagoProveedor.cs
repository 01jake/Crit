using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class PagoProveedor
    {
        public int Id { get; set; }

        [Required]
        public int CuentaPorPagarId { get; set; }
        public CuentaPorPagar? CuentaPorPagar { get; set; }

        public DateTime FechaPago { get; set; } = DateTime.Now;

        [Range(typeof(decimal), "0.01", "999999999999")]
        public decimal Monto { get; set; }

        [StringLength(50)]
        public string? MetodoPago { get; set; }

        [StringLength(100)]
        public string? Referencia { get; set; }

        public decimal SaldoAnterior { get; set; }
        public decimal SaldoPosterior { get; set; }

        [StringLength(250)]
        public string? Observaciones { get; set; }

        public string? UsuarioId { get; set; }

        public bool Activo { get; set; } = true;
    }
}

