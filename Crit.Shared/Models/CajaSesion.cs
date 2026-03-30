using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class CajaSesion
    {
        public int Id { get; set; }

        public DateTime FechaApertura { get; set; } = DateTime.Now;
        public DateTime? FechaCierre { get; set; }

        public decimal MontoInicial { get; set; }
        public decimal MontoFinal { get; set; }

        public decimal TotalIngresos { get; set; }
        public decimal TotalEgresos { get; set; }

        public decimal SaldoCalculado => MontoInicial + TotalIngresos - TotalEgresos;

        [StringLength(30)]
        public string Estado { get; set; } = "Abierta";
        // Abierta, Cerrada, Cancelada

        public string? UsuarioId { get; set; }

        [StringLength(250)]
        public string? Observaciones { get; set; }

        public ICollection<CajaMovimiento> Movimientos { get; set; } = new List<CajaMovimiento>();
    }
}
