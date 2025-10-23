using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class EmpleadoEstadisticasDto
    {
        public string EmpleadoId { get; set; }
        public string EmpleadoNombre { get; set; }
        public int TotalAsignadas { get; set; }
        public int Pendientes { get; set; }
        public int EnProceso { get; set; }
        public int Resueltas { get; set; }
        public List<Queja> Quejas { get; set; }
    }
}
