using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class OrdenReabastecimiento
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;

        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        public int AlmacenId { get; set; }
        public Almacen? Almacen { get; set; }

        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }
        public decimal CantidadSugerida { get; set; }

        public string Estado { get; set; } = "Pendiente";
        public string? TipoSugerido { get; set; }
        public string? Observaciones { get; set; }
    }

}
