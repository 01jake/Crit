using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class OrdenReabastecimiento
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        [Required]
        public int AlmacenId { get; set; }
        public Almacen? Almacen { get; set; }

        public int? CompraId { get; set; }
        public Compra? Compra { get; set; }

        public int? TraspasoAlmacenId { get; set; }
        public TraspasoAlmacen? TraspasoAlmacen { get; set; }
        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }
        public decimal CantidadSugerida { get; set; }


        [StringLength(30)]
        public string Estado { get; set; } = "Pendiente";
        // Pendiente, Solicitada, EnProceso, Completada, Cancelada

        [StringLength(50)]
        public string? TipoSugerido { get; set; }
        // Compra, Traspaso, Ajuste

        [StringLength(250)]
        public string? Observaciones { get; set; }
    }

}
