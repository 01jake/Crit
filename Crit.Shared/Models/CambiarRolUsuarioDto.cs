using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class CambiarRolUsuarioDto
    {
        [Required(ErrorMessage = "El rol es obligatorio.")]
        public string Rol { get; set; } = string.Empty;
    }
}
