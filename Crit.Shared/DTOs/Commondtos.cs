using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.DTOs
{
    public class DashboardStatsDto
    {
        public decimal TotalVentasMes { get; set; }
        public int TotalClientes { get; set; }
        public int TotalProductos { get; set; }
        public int ProductosBajoStock { get; set; }
        public decimal IngresosMes { get; set; }
        public decimal GananciaMes { get; set; }
        public decimal TicketPromedio { get; set; }

        // VENTAS
        public int VentasHoy { get; set; }
        public int VentasMes { get; set; }

        // INVENTARIO
        public decimal ValorInventario { get; set; }
        public decimal CostosMes { get; set; }
        public decimal UtilidadMes { get; set; }
        public decimal MargenUtilidad { get; set; }




        public decimal IngresosHoy { get; set; }
  

        public decimal CostoVentasHoy { get; set; }
        public decimal CostoVentasMes { get; set; }

        public decimal UtilidadBrutaHoy { get; set; }
        public decimal UtilidadBrutaMes { get; set; }

        public decimal MargenBrutoHoy { get; set; }
        public decimal MargenBrutoMes { get; set; }

        public decimal TicketPromedioHoy { get; set; }
        public decimal TicketPromedioMes { get; set; }

   
    }

    public class VentasPorMesDto
    {
        public string Mes { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int Cantidad { get; set; }
    }

    public class CrearVentaDto
    {
        public int ClienteId { get; set; }
        public decimal Descuento { get; set; }
        public string? Notas { get; set; }
        public List<DetalleVentaDto> Detalles { get; set; } = new();
    }

    public class DetalleVentaDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Descuento { get; set; }
    }

    public class ProductoMasVendidoDto
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal TotalVentas { get; set; }


    }

    // DTO para reportes
    public class ReporteVentasDto
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal TotalVentas { get; set; }
        public int NumeroVentas { get; set; }
        public decimal PromedioVenta { get; set; }
        public List<VentaPorDiaDto> VentasPorDia { get; set; } = new();
    }

    public class VentaPorDiaDto
    {
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public int Cantidad { get; set; }
    }

    // DTO para respuestas de API
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    // DTO simplificado para listas
    public class ClienteListDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class ProductoListDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal PrecioVenta { get; set; }
        public int Stock { get; set; }
        public bool Activo { get; set; }
    }
    public class CashFlowDto
    {
        public string Mes { get; set; } = string.Empty;
        public decimal Ingresos { get; set; }
        public decimal CostoMercancia { get; set; }
        public decimal UtilidadBruta { get; set; }
        public decimal FlujoEstimado { get; set; }
    }



    public class VentasPorDiaDto
    {
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public int Cantidad { get; set; }
    }


    public class DashboardAlertaDto
    {
        public List<ProductoBajoStockDto> ProductosBajoStock { get; set; } = new();
        public List<string> Mensajes { get; set; } = new();
    }

    public class ProductoBajoStockDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class VentaRecienteDto
    {
        public int Id { get; set; }
        public string NumeroVenta { get; set; } = string.Empty;
        public string? Cliente { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
    public class CuentaPorCobrarResumenDto
    {
        public int Id { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string? Folio { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public decimal Total { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal Saldo { get; set; }
        public string Estado { get; set; } = string.Empty;
        public bool EstaVencida { get; set; }
        public int DiasVencidos { get; set; }
    }

    public class CuentaPorPagarResumenDto
    {
        public int Id { get; set; }
        public string Proveedor { get; set; } = string.Empty;
        public string? FolioFactura { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public decimal Total { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal Saldo { get; set; }
        public string Estado { get; set; } = string.Empty;
        public bool EstaVencida { get; set; }
        public int DiasVencidos { get; set; }
    }

    public class RegistrarPagoClienteDto
    {
        public DateTime FechaPago { get; set; } = DateTime.Now;
        public decimal Monto { get; set; }
        public string? MetodoPago { get; set; }
        public string? Referencia { get; set; }
        public string? Observaciones { get; set; }
    }

    public class RegistrarPagoProveedorDto
    {
        public DateTime FechaPago { get; set; } = DateTime.Now;
        public decimal Monto { get; set; }
        public string? MetodoPago { get; set; }
        public string? Referencia { get; set; }
        public string? Observaciones { get; set; }
    }

    public class FinanzasResumenDto
    {
        public decimal TotalPorCobrar { get; set; }
        public decimal TotalPorPagar { get; set; }
        public decimal TotalCobradoMes { get; set; }
        public decimal TotalPagadoMes { get; set; }
        public decimal CarteraVencidaClientes { get; set; }
        public decimal CarteraVencidaProveedores { get; set; }
        public int CuentasPorCobrarPendientes { get; set; }
        public int CuentasPorPagarPendientes { get; set; }

    }
    public class CajaResumenDto
    {
        public bool CajaAbierta { get; set; }
        public decimal MontoInicial { get; set; }
        public decimal IngresosHoy { get; set; }
        public decimal EgresosHoy { get; set; }
        public decimal SaldoActual { get; set; }
    }

    public class FlujoCajaRealDto
    {
        public string Periodo { get; set; } = string.Empty;
        public decimal Ingresos { get; set; }
        public decimal Egresos { get; set; }
        public decimal Neto { get; set; }
    }

    public class RegistrarGastoDto
    {
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Concepto { get; set; } = string.Empty;
        public string? Categoria { get; set; }
        public decimal Monto { get; set; }
        public string? MetodoPago { get; set; }
        public string? Referencia { get; set; }
        public string? Observaciones { get; set; }
        public int? ProveedorId { get; set; }
    }

    public class AperturaCajaDto
    {
        public decimal MontoInicial { get; set; }
        public string? Observaciones { get; set; }
    }

    public class CierreCajaDto
    {
        public decimal MontoFinal { get; set; }
        public string? Observaciones { get; set; }
    }
    public class CrearCompraDesdeReabastecimientoDto
    {
        public int OrdenReabastecimientoId { get; set; }
        public int ProveedorId { get; set; }
        public int AlmacenId { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string? SerieFactura { get; set; }
        public string? FolioFactura { get; set; }
        public string? RFCProveedor { get; set; }
        public DateTime FechaFactura { get; set; } = DateTime.Today;
        public bool EsCredito { get; set; }
        public int? DiasCredito { get; set; }
    }
    public class KardexMovimientoDto
    {
        public DateTime Fecha { get; set; }
        public string Producto { get; set; } = string.Empty;
        public string? CodigoProducto { get; set; }
        public string Almacen { get; set; } = string.Empty;
        public string TipoMovimiento { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal StockAnterior { get; set; }
        public decimal StockNuevo { get; set; }
        public string? Referencia { get; set; }
        public string? Observaciones { get; set; }
    }
    public class EmpresaSesionDto
    {
        public int EmpresaId { get; set; }
        public string EmpresaNombre { get; set; } = string.Empty;
        public string? Email { get; set; }
    }
    public class RegisterEmpresaDto
    {
        [Required]
        [StringLength(150)]
        public string EmpresaNombre { get; set; } = string.Empty;

        [StringLength(20)]
        public string? RFC { get; set; }

        [Required]
        [StringLength(100)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
    public class CrearUsuarioDto
    {
        [Required]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Rol { get; set; } = "Usuario";
    }
}
