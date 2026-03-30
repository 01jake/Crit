using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class InventarioPorAlmacen
    {
        public int Id { get; set; }

        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        public int AlmacenId { get; set; }
        public Almacen? Almacen { get; set; }

        public int? UbicacionAlmacenId { get; set; }
        public UbicacionAlmacen? UbicacionAlmacen { get; set; }

        public decimal Stock { get; set; }
        public decimal StockMinimo { get; set; }
        public decimal StockMaximo { get; set; }
    }
}
