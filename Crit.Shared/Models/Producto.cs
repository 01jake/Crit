using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Crit.Shared.Models;



namespace Crit.Shared.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }


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
        public decimal PrecioCompra { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioVenta { get; set; }

        [Required]
        public int Stock { get; set; }

        [Required]
        public int StockMinimo { get; set; } = 5;

        [StringLength(50)]
        public string? Categoria { get; set; }

        [StringLength(50)]
        public string? Unidad { get; set; } = "Pieza";

        public int? ProveedorId { get; set; }
        public Proveedor? Proveedor { get; set; }



        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Relaciones
        public ICollection<DetalleVenta>? DetallesVenta { get; set; }
        public ICollection<DetalleCotizacion>? DetallesCotizacion { get; set; }
    }

}