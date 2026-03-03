using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class Servicio
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código es requerido")]
        [StringLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        [StringLength(50)]
        public string? Categoria { get; set; }

        [Range(0, 500)]
        public int TiempoEstimadoHoras { get; set; } // Duración estimada en horas

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Relaciones
        public ICollection<ServicioCliente>? ServiciosCliente { get; set; }
    }
}
