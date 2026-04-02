using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class InventarioPorAlmacen
    {
        public int Id { get; set; }
        public int? EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }

        [Required]
        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }
        [Required]
        public int AlmacenId { get; set; }
        public Almacen? Almacen { get; set; }
        public int? UbicacionAlmacenId { get; set; }
        public UbicacionAlmacen? UbicacionAlmacen { get; set; }

        // Agregar las siguientes propiedades para corregir el error CS1061
        public decimal Stock { get; set; }
        public decimal StockMinimo { get; set; }
        public decimal StockMaximo { get; set; }
    }
}
