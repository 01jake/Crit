using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class Entrega
    {
        public int Id { get; set; }

        public int VentaId { get; set; }
        public Venta? Venta { get; set; }

        public string? Ruta { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public DateTime? FechaSalida { get; set; }
        public DateTime? FechaEstimadaEntrega { get; set; }
        public DateTime? FechaEntregaReal { get; set; }

        public string? DireccionEntrega { get; set; }
        public string? Repartidor { get; set; }
        public string? Observaciones { get; set; }
    }

}
