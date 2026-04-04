using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class ServicioCliente
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }

        [Required]
        public int ServicioId { get; set; }
        public Servicio? Servicio { get; set; }

        [Required]
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; } = DateTime.Now;

        public DateTime? FechaFinalizacion { get; set; }

        [Required]
        [StringLength(50)]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, En Proceso, Completado, Cancelado

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioFinal { get; set; }

        [StringLength(1000)]
        public string? Notas { get; set; }

        public string UsuarioId { get; set; } = string.Empty;
    }
}
