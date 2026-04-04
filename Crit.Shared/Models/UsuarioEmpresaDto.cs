using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class UsuarioEmpresaDto
    {
        public string Id { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int EmpresaId { get; set; }
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}
