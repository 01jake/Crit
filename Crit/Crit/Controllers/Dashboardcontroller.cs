using Crit.Server.Data;
using Crit.Shared.DTOs;
using Crit.Shared.Models;
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
        private readonly IEmpresaProvider _empresaProvider;

        public DashboardController(
            ApplicationDbContext context,
            ILogger<DashboardController> logger,
            IEmpresaProvider empresaProvider)
        {
            _context = context;
            _logger = logger;
            _empresaProvider = empresaProvider;
        }

        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStatsDto>> GetStats([FromQuery] DateTime? fechaInicio = null)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var ventas = _context.Ventas
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Where(v => v.EmpresaId == empresaId);

                if (fechaInicio.HasValue)
                    ventas = ventas.Where(v => v.Fecha >= fechaInicio.Value);

                var ventasList = await ventas.ToListAsync();

                var ingresos = ventasList.Sum(v => v.Total);
                var costoVentas = ventasList.Sum(v => v.Detalles.Sum(d => d.Cantidad * d.Producto!.PrecioCompra));
                var utilidadBruta = ingresos - costoVentas;
                var margen = ingresos > 0 ? (utilidadBruta / ingresos) * 100 : 0;

                var productos = await _context.Productos
                    .Where(p => p.EmpresaId == empresaId)
                    .ToListAsync();

                var valorInventario = productos.Sum(p => p.Stock * p.PrecioCompra);

                return Ok(new DashboardStatsDto
                {
                    IngresosMes = ingresos,
                    CostoVentasMes = costoVentas,
                    UtilidadBrutaMes = utilidadBruta,
                    MargenBrutoMes = margen,
                    ValorInventario = valorInventario,
                    TotalProductos = productos.Count
                });
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
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var fechaInicio = DateTime.Today.AddMonths(-meses);

                var ventas = await _context.Ventas
                    .AsNoTracking()
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Where(v => v.EmpresaId == empresaId && v.Fecha >= fechaInicio)
                    .ToListAsync();

                var data = ventas
                    .GroupBy(v => new { v.Fecha.Year, v.Fecha.Month })
                    .Select(g =>
                    {
                        var ingresos = g.Sum(v => v.Total);
                        var costoMercancia = g.Sum(v => v.Detalles.Sum(d => d.Cantidad * (d.Producto?.PrecioCompra ?? 0m)));
                        var utilidadBruta = ingresos - costoMercancia;

                        return new CashFlowDto
                        {
                            Mes = $"{g.Key.Month:00}/{g.Key.Year}",
                            Ingresos = ingresos,
                            CostoMercancia = costoMercancia,
                            UtilidadBruta = utilidadBruta,
                            FlujoEstimado = utilidadBruta
                        };
                    })
                    .OrderBy(x => x.Mes)
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
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var fechaInicio = DateTime.Today.AddDays(-dias);

                var ventas = await _context.Ventas
                    .AsNoTracking()
                    .Where(v => v.EmpresaId == empresaId && v.Fecha >= fechaInicio)
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
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var data = await _context.DetallesVenta
                    .AsNoTracking()
                    .Include(d => d.Producto)
                    .Include(d => d.Venta)
                    .Where(d => d.Venta != null && d.Venta.EmpresaId == empresaId)
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
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var productosBajoStock = await _context.Productos
                    .AsNoTracking()
                    .Where(p => p.EmpresaId == empresaId && p.Stock <= p.StockMinimo)
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
        public async Task<ActionResult<IEnumerable<VentaRecienteDto>>> GetVentasRecientes([FromQuery] int take = 5)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var ventas = await _context.Ventas
                    .Include(v => v.Cliente)
                    .Where(v => v.EmpresaId == empresaId)
                    .OrderByDescending(v => v.Fecha)
                    .Take(take)
                    .Select(v => new VentaRecienteDto
                    {
                        NumeroVenta = v.NumeroVenta,
                        Cliente = v.Cliente != null ? v.Cliente.Nombre : "Público General",
                        Fecha = v.Fecha,
                        Total = v.Total
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

        [HttpGet("finanzas-resumen")]
        public async Task<ActionResult<FinanzasResumenDto>> GetFinanzasResumen()
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

                var cuentasPorCobrar = await _context.CuentasPorCobrar
                    .Where(x => x.EmpresaId == empresaId && x.Activa)
                    .ToListAsync();

                var cuentasPorPagar = await _context.CuentasPorPagar
                    .Where(x => x.EmpresaId == empresaId && x.Activa)
                    .ToListAsync();

                var pagosCliente = await _context.Set<PagoCliente>()
                    .Where(x => x.EmpresaId == empresaId && x.Activo && x.FechaPago >= inicioMes)
                    .ToListAsync();

                var pagosProveedor = await _context.Set<PagoProveedor>()
                    .Where(x => x.EmpresaId == empresaId && x.Activo && x.FechaPago >= inicioMes)
                    .ToListAsync();

                return Ok(new FinanzasResumenDto
                {
                    TotalPorCobrar = cuentasPorCobrar.Where(x => x.Saldo > 0).Sum(x => x.Saldo),
                    TotalPorPagar = cuentasPorPagar.Where(x => x.Saldo > 0).Sum(x => x.Saldo),
                    CuentasPorCobrarPendientes = cuentasPorCobrar.Count(x => x.Saldo > 0),
                    CuentasPorPagarPendientes = cuentasPorPagar.Count(x => x.Saldo > 0),
                    TotalCobradoMes = pagosCliente.Sum(x => x.Monto),
                    TotalPagadoMes = pagosProveedor.Sum(x => x.Monto)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener resumen financiero");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
