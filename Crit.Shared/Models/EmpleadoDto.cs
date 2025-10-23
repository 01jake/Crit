using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class EmpleadoDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int QuejasAsignadas { get; set; }
        public int QuejasEnProceso { get; set; }
        public int QuejasResueltas { get; set; }
    }
}
