using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class MovimientoInventario
    {
        public int Id { get; set; }
        public int? EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        public int AlmacenId { get; set; }
        public Almacen? Almacen { get; set; }

        public int? UbicacionAlmacenId { get; set; }
        public UbicacionAlmacen? UbicacionAlmacen { get; set; }

        public string TipoMovimiento { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }

        public decimal StockAnterior { get; set; }
        public decimal StockNuevo { get; set; }

        public string? Referencia { get; set; }
        public string? Observaciones { get; set; }

        public int? CompraId { get; set; }
        public int? VentaId { get; set; }
        public int? TraspasoId { get; set; }
    }

}
