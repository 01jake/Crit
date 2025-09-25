using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Crit.Data;

namespace Crit.Server.Data
{
    [Table("Articulos")]
    public class ArticuloEntity
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;
        [Required(ErrorMessage = "El Codigo es obligatoria.")]
        [StringLength(50)]

        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La Descripcion es obligatoria.")]
        [StringLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La Ubicacion es obligatoria.")]
        [StringLength(200)]
        public string Ubicacion { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Uso { get; set; } = string.Empty;

        public NivelPriorizacion NivelPriorizacion { get; set; } = NivelPriorizacion.Medio;

        [Required(ErrorMessage = "La Fecha es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [Required]
        public string UsuarioQueRegistroId { get; set; } = string.Empty;

        [ForeignKey(nameof(UsuarioQueRegistroId))]
        public virtual ApplicationUser? UsuarioQueRegistro { get; set; }
    }
}