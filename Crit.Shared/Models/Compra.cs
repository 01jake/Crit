using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Crit.Shared.Models
{
    public class Compra
    {
        public int Id { get; set; }
        public int ProveedorId { get; set; }
        public Proveedor? Proveedor { get; set; }

        public DateTime Fecha { get; set; }

        // FACTURA PROVEEDOR
        public string? SerieFactura { get; set; }
        public string? FolioFactura { get; set; }
        public string? RFCProveedor { get; set; }
        public DateTime FechaFactura { get; set; }
        public bool EsCredito { get; set; } = false;
        public int? DiasCredito { get; set; }

        public decimal Subtotal { get; set; }
        public decimal IVA { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = "Completada";
      
        public List<DetalleCompra> Detalles { get; set; } = new();
        public CuentaPorPagar? CuentaPorPagar { get; set; }


    }
}
