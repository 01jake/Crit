using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class TraspasoAlmacen
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public int AlmacenOrigenId { get; set; }
        public int AlmacenDestinoId { get; set; }

        public Almacen? AlmacenOrigen { get; set; }
        public Almacen? AlmacenDestino { get; set; }

        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        public decimal Cantidad { get; set; }
        [StringLength(30)]
        public string Estado { get; set; } = "Pendiente";
        [StringLength(250)]
        public string? Observaciones { get; set; }
    }

}
