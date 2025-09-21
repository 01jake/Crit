using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Crit.Data;

namespace Crit.Server.Data
{
    public enum EstatusQueja
    {
        Pendiente = 0,
        Atendida = 1,
        Cerrada = 2
    }

    public enum PrioridadQueja
    {
        Baja = 0,
        Media = 1,
        Alta = 2
    }

    [Table("Quejas")]
    public class QuejaEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string NombreCliente { get; set; } = string.Empty;

        [StringLength(50)]
        public string NumeroAfiliacion { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string Correo { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string DescripcionQueja { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Categoria { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; } = DateTime.Now;

        public EstatusQueja Estatus { get; set; } = EstatusQueja.Pendiente;

        public PrioridadQueja Prioridad { get; set; } = PrioridadQueja.Media;

        // Relación con ApplicationUser (Cliente que reporta)
        [Required]
        public string ClienteId { get; set; } = string.Empty;

        [ForeignKey(nameof(ClienteId))]
        public virtual ApplicationUser? Cliente { get; set; }
    }
}