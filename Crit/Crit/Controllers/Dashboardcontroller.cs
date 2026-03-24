using Crit.Server.Data;
using Crit.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStatsDto>> GetStats()
        {
            try
            {
                var hoy = DateTime.Today;
                var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

                var ventasHoy = await _context.Ventas
                    .AsNoTracking()
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Where(v => v.Fecha.Date == hoy)
                    .ToListAsync();

                var ventasMes = await _context.Ventas
                    .AsNoTracking()
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Where(v => v.Fecha >= inicioMes)
                    .ToListAsync();

                var productos = await _context.Productos
                    .AsNoTracking()
                    .ToListAsync();

                decimal ingresosHoy = ventasHoy.Sum(v => v.Total);
                decimal ingresosMes = ventasMes.Sum(v => v.Total);

                decimal costoVentasHoy = ventasHoy.Sum(v =>
                    v.Detalles.Sum(d => d.Cantidad * (d.Producto?.PrecioCompra ?? 0m)));

                decimal costoVentasMes = ventasMes.Sum(v =>
                    v.Detalles.Sum(d => d.Cantidad * (d.Producto?.PrecioCompra ?? 0m)));

                decimal utilidadBrutaHoy = ingresosHoy - costoVentasHoy;
                decimal utilidadBrutaMes = ingresosMes - costoVentasMes;

                decimal margenBrutoHoy = ingresosHoy > 0 ? (utilidadBrutaHoy / ingresosHoy) * 100m : 0m;
                decimal margenBrutoMes = ingresosMes > 0 ? (utilidadBrutaMes / ingresosMes) * 100m : 0m;

                decimal ticketPromedioHoy = ventasHoy.Count > 0 ? ingresosHoy / ventasHoy.Count : 0m;
                decimal ticketPromedioMes = ventasMes.Count > 0 ? ingresosMes / ventasMes.Count : 0m;

                int productosBajoStock = productos.Count(p => p.Stock <= p.StockMinimo);

                decimal valorInventario = productos.Sum(p => p.Stock * p.PrecioCompra);

                var stats = new DashboardStatsDto
                {
                    IngresosHoy = ingresosHoy,
                    IngresosMes = ingresosMes,
                    CostoVentasHoy = costoVentasHoy,
                    CostoVentasMes = costoVentasMes,
                    UtilidadBrutaHoy = utilidadBrutaHoy,
                    UtilidadBrutaMes = utilidadBrutaMes,
                    MargenBrutoHoy = margenBrutoHoy,
                    MargenBrutoMes = margenBrutoMes,
                    TicketPromedioHoy = ticketPromedioHoy,
                    TicketPromedioMes = ticketPromedioMes,
                    VentasHoy = ventasHoy.Count,
                    VentasMes = ventasMes.Count,
                    TotalClientes = await _context.Clientes.CountAsync(),
                    TotalProductos = productos.Count,
                    ProductosBajoStock = productosBajoStock,
                    ValorInventario = valorInventario
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener stats del dashboard");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("cash-flow")]
        public async Task<ActionResult<List<CashFlowDto>>> GetCashFlow([FromQuery] int meses = 6)
        {
            try
            {
                var fechaInicio = DateTime.Today.AddMonths(-meses);

                var ventas = await _context.Ventas
                    .AsNoTracking()
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Where(v => v.Fecha >= fechaInicio)
                    .ToListAsync();

                var data = ventas
                    .GroupBy(v => new { v.Fecha.Year, v.Fecha.Month })
                    .Select(g =>
                    {
                        var ingresos = g.Sum(v => v.Total);
                        var costoMercancia = g.Sum(v =>
                            v.Detalles.Sum(d => d.Cantidad * (d.Producto?.PrecioCompra ?? 0m)));

                        var utilidadBruta = ingresos - costoMercancia;

                        return new
                        {
                            g.Key.Year,
                            g.Key.Month,
                            Ingresos = ingresos,
                            CostoMercancia = costoMercancia,
                            UtilidadBruta = utilidadBruta
                        };
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .Select(x => new CashFlowDto
                    {
                        Mes = $"{x.Month:00}/{x.Year}",
                        Ingresos = x.Ingresos,
                        CostoMercancia = x.CostoMercancia,
                        UtilidadBruta = x.UtilidadBruta,
                        FlujoEstimado = x.UtilidadBruta
                    })
                    .ToList();

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cash flow");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("ventas-por-dia")]
        public async Task<ActionResult<List<VentasPorDiaDto>>> GetVentasPorDia([FromQuery] int dias = 30)
        {
            try
            {
                var fechaInicio = DateTime.Today.AddDays(-dias);

                var ventas = await _context.Ventas
                    .AsNoTracking()
                    .Where(v => v.Fecha >= fechaInicio)
                    .GroupBy(v => v.Fecha.Date)
                    .Select(g => new VentasPorDiaDto
                    {
                        Fecha = g.Key,
                        Total = g.Sum(v => v.Total),
                        Cantidad = g.Count()
                    })
                    .OrderBy(x => x.Fecha)
                    .ToListAsync();

                return Ok(ventas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas por día");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("productos-mas-vendidos")]
        public async Task<ActionResult<List<ProductoMasVendidoDto>>> GetProductosMasVendidos([FromQuery] int cantidad = 5)
        {
            try
            {
                var data = await _context.DetallesVenta
                    .AsNoTracking()
                    .Include(d => d.Producto)
                    .GroupBy(d => new
                    {
                        d.ProductoId,
                        Nombre = d.Producto!.Nombre
                    })
                    .Select(g => new ProductoMasVendidoDto
                    {
                        ProductoId = g.Key.ProductoId,
                        Nombre = g.Key.Nombre,
                        CantidadVendida = g.Sum(x => x.Cantidad),
                        TotalVentas = g.Sum(x => x.Subtotal)
                    })
                    .OrderByDescending(x => x.CantidadVendida)
                    .Take(cantidad)
                    .ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos más vendidos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("alertas")]
        public async Task<ActionResult<DashboardAlertaDto>> GetAlertas()
        {
            try
            {
                var productosBajoStock = await _context.Productos
                    .AsNoTracking()
                    .Where(p => p.Stock <= p.StockMinimo)
                    .OrderBy(p => p.Stock)
                    .Select(p => new ProductoBajoStockDto
                    {
                        Id = p.Id,
                        Codigo = p.Codigo,
                        Nombre = p.Nombre,
                        Stock = p.Stock,
                        StockMinimo = p.StockMinimo,
                        Estado = p.Stock == 0 ? "Sin stock" : "Crítico"
                    })
                    .ToListAsync();

                var mensajes = new List<string>();

                if (productosBajoStock.Any())
                    mensajes.Add($"Hay {productosBajoStock.Count} productos con stock bajo.");

                return Ok(new DashboardAlertaDto
                {
                    ProductosBajoStock = productosBajoStock,
                    Mensajes = mensajes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener alertas");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("ventas-recientes")]
        public async Task<ActionResult<List<VentaRecienteDto>>> GetVentasRecientes([FromQuery] int cantidad = 5)
        {
            try
            {
                var ventas = await _context.Ventas
                    .AsNoTracking()
                    .Include(v => v.Cliente)
                    .OrderByDescending(v => v.Fecha)
                    .Take(cantidad)
                    .Select(v => new VentaRecienteDto
                    {
                        Id = v.Id,
                        NumeroVenta = v.NumeroVenta,
                        Cliente = v.Cliente != null ? v.Cliente.Nombre : null,
                        Fecha = v.Fecha,
                        Total = v.Total,
                        Estado = v.Estado
                    })
                    .ToListAsync();

                return Ok(ventas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas recientes");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
