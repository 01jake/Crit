using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class UbicacionAlmacen
    {
        public int Id { get; set; }
        public int AlmacenId { get; set; }
        public Almacen? Almacen { get; set; }

        public string Codigo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activa { get; set; } = true;
    }
}
