using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class Empresa
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Plan { get; set; } = "Free"; 
    }
}
