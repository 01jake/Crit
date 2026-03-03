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

        // GET: api/dashboard/stats
        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStatsDto>> GetStats()
        {
            try
            {
                var mesActual = DateTime.Now.Month;
                var añoActual = DateTime.Now.Year;

                var totalVentasMes = await _context.Ventas
                    .Where(v => v.Fecha.Month == mesActual &&
                                v.Fecha.Year == añoActual &&
                                v.Estado == "Completada")
                    .SumAsync(v => v.Total);

                var totalClientes = await _context.Clientes.CountAsync(c => c.Activo);

                var totalProductos = await _context.Productos.CountAsync(p => p.Activo);

                var productosBajoStock = await _context.Productos
                    .CountAsync(p => p.Stock <= p.StockMinimo && p.Activo);

                var stats = new DashboardStatsDto
                {
                    TotalVentasMes = totalVentasMes,
                    TotalClientes = totalClientes,
                    TotalProductos = totalProductos,
                    ProductosBajoStock = productosBajoStock
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas del dashboard");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/dashboard/ventas-por-mes?meses=6
        [HttpGet("ventas-por-mes")]
        public async Task<ActionResult<List<VentasPorMesDto>>> GetVentasPorMes([FromQuery] int meses = 6)
        {
            try
            {
                var resultado = new List<VentasPorMesDto>();

                for (int i = meses - 1; i >= 0; i--)
                {
                    var fecha = DateTime.Now.AddMonths(-i);

                    var total = await _context.Ventas
                        .Where(v => v.Fecha.Month == fecha.Month &&
                                    v.Fecha.Year == fecha.Year &&
                                    v.Estado == "Completada")
                        .SumAsync(v => v.Total);

                    var cantidad = await _context.Ventas
                        .CountAsync(v => v.Fecha.Month == fecha.Month &&
                                        v.Fecha.Year == fecha.Year &&
                                        v.Estado == "Completada");

                    resultado.Add(new VentasPorMesDto
                    {
                        Mes = fecha.ToString("MMM yyyy"),
                        Total = total,
                        Cantidad = cantidad
                    });
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas por mes");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/dashboard/productos-mas-vendidos?cantidad=5
        [HttpGet("productos-mas-vendidos")]
        public async Task<ActionResult<List<ProductoMasVendidoDto>>> GetProductosMasVendidos([FromQuery] int cantidad = 5)
        {
            try
            {
                var desde = DateTime.Now.AddMonths(-1);
                var hasta = DateTime.Now;

                var productosMasVendidos = await _context.DetallesVenta
                    .Include(d => d.Producto)
                    .Include(d => d.Venta)
                    .Where(d => d.Venta.Fecha >= desde &&
                                d.Venta.Fecha <= hasta &&
                                d.Venta.Estado == "Completada")
                    .GroupBy(d => new { d.ProductoId, d.Producto.Nombre })
                    .Select(g => new ProductoMasVendidoDto
                    {
                        ProductoId = g.Key.ProductoId,
                        Nombre = g.Key.Nombre,
                        CantidadVendida = g.Sum(d => d.Cantidad),
                        TotalVentas = g.Sum(d => d.Subtotal)
                    })
                    .OrderByDescending(p => p.CantidadVendida)
                    .Take(cantidad)
                    .ToListAsync();

                return Ok(productosMasVendidos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos más vendidos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/dashboard/alertas
        [HttpGet("alertas")]
        public async Task<ActionResult<object>> GetAlertas()
        {
            try
            {
                var productosBajoStock = await _context.Productos
                    .Where(p => p.Stock <= p.StockMinimo && p.Activo)
                    .Select(p => new
                    {
                        p.Id,
                        p.Codigo,
                        p.Nombre,
                        p.Stock,
                        p.StockMinimo
                    })
                    .ToListAsync();

                var alertas = new
                {
                    ProductosBajoStock = productosBajoStock
                };

                return Ok(alertas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener alertas");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
