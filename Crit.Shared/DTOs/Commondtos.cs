using System;
using System.Collections.Generic;
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

    //public class DashboardStatsDto
    //{
    //    public decimal TotalVentasMes { get; set; }
    //    public int TotalClientes { get; set; }
    //    public int TotalProductos { get; set; }
    //    public int ProductosBajoStock { get; set; }
    //}

}
