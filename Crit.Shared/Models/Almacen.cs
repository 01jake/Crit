using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class Almacen
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Clave { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; } = true;

        public ICollection<UbicacionAlmacen> Ubicaciones { get; set; } = new List<UbicacionAlmacen>();
    }
}
