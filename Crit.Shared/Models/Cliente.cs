using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public int? EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }
        [Required(ErrorMessage = "El nombre o razón social es requerido")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Teléfono inválido")]
        public string? Telefono { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }

        [StringLength(13, MinimumLength = 12, ErrorMessage = "El RFC debe tener entre 12 y 13 caracteres")]
        // Esta Regex acepta: 4 letras, 6 números y 3 caracteres de homoclave (Mayúsculas o Minúsculas)
        [RegularExpression(@"^([A-Za-zñÑ&]{3,4})([0-9]{2}(0[1-9]|1[0-2])(0[1-9]|[12][0-9]|3[01]))([A-Za-z0-9]{3})$",
     ErrorMessage = "Formato de RFC inválido. Ejemplo: VIML030903QY6")]
        public string? RFC { get; set; }

        [Required(ErrorMessage = "El Código Postal es obligatorio para facturación")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "El CP debe ser de 5 dígitos")]
        [RegularExpression(@"^[0-9]{5}$", ErrorMessage = "CP inválido")]
        public string? CodigoPostal { get; set; }

        [StringLength(100)]
        public string? RegimenFiscal { get; set; } 

        [StringLength(100)]
        public string? UsoCFDI { get; set; } 

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public bool Activo { get; set; } = true;

        // Relaciones
        public ICollection<Venta>? Ventas { get; set; }
        public ICollection<Cotizacion>? Cotizaciones { get; set; }
        public ICollection<CuentaPorCobrar>? CuentasPorCobrar { get; set; }
        

    }
}
