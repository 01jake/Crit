using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class CrearTraspasoDesdeReabastecimientoDto
    {
        public int OrdenReabastecimientoId { get; set; }
        public int AlmacenOrigenId { get; set; }
        public int AlmacenDestinoId { get; set; }
        public int ProductoId { get; set; }
        public decimal Cantidad { get; set; }
        public string? Observaciones { get; set; }
    }
}
