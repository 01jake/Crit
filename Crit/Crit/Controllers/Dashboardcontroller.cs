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
        public async Task<ActionResult<DashboardStatsDto>> GetStats([FromQuery] string? fechaInicio = null)
        {
            try
            {
                var hoy = DateTime.Today;
                DateTime inicioFiltro;

                // Si el cliente envía una fecha (7 días, hoy, etc.), la usamos.
                // Si no envía nada o el formato es incorrecto, usamos el primero de mes por defecto.
                if (string.IsNullOrEmpty(fechaInicio) || !DateTime.TryParse(fechaInicio, out inicioFiltro))
                {
                    inicioFiltro = new DateTime(hoy.Year, hoy.Month, 1);
                }

                // 1. VENTAS DEL PERIODO FILTRADO (Dinámico)
                var ventasPeriodo = await _context.Ventas
                    .AsNoTracking()
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Where(v => v.Fecha >= inicioFiltro)
                    .ToListAsync();

                // 2. VENTAS DE HOY (Estático, para el cuadro de "Hoy")
                var ventasHoy = await _context.Ventas
                    .AsNoTracking()
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Where(v => v.Fecha.Date == hoy)
                    .ToListAsync();

                var productos = await _context.Productos.AsNoTracking().ToListAsync();

                // CÁLCULOS DEL PERIODO (Los que cambian con el botón)
                decimal ingresosPeriodo = ventasPeriodo.Sum(v => v.Total);
                decimal costoVentasPeriodo = ventasPeriodo.Sum(v =>
                    v.Detalles.Sum(d => d.Cantidad * (d.Producto?.PrecioCompra ?? 0m)));
                decimal utilidadBrutaPeriodo = ingresosPeriodo - costoVentasPeriodo;

                // CÁLCULOS DE HOY (Los que siempre muestran el día actual)
                decimal ingresosHoy = ventasHoy.Sum(v => v.Total);
                decimal costoVentasHoy = ventasHoy.Sum(v =>
                    v.Detalles.Sum(d => d.Cantidad * (d.Producto?.PrecioCompra ?? 0m)));

                var stats = new DashboardStatsDto
                {
                    // Mapeamos los datos filtrados a las propiedades que usa el Dash
                    IngresosMes = ingresosPeriodo,
                    CostoVentasMes = costoVentasPeriodo,
                    UtilidadBrutaMes = utilidadBrutaPeriodo,
                    MargenBrutoMes = ingresosPeriodo > 0 ? (utilidadBrutaPeriodo / ingresosPeriodo) * 100m : 0m,
                    VentasMes = ventasPeriodo.Count,
                    TicketPromedioMes = ventasPeriodo.Count > 0 ? ingresosPeriodo / ventasPeriodo.Count : 0m,

                    // Datos de hoy
                    IngresosHoy = ingresosHoy,
                    CostoVentasHoy = costoVentasHoy,
                    UtilidadBrutaHoy = ingresosHoy - costoVentasHoy,
                    VentasHoy = ventasHoy.Count,
                    TicketPromedioHoy = ventasHoy.Count > 0 ? ingresosHoy / ventasHoy.Count : 0m,

                    // Datos Globales
                    TotalClientes = await _context.Clientes.CountAsync(),
                    TotalProductos = productos.Count,
                    ProductosBajoStock = productos.Count(p => p.Stock <= p.StockMinimo),
                    ValorInventario = productos.Sum(p => p.Stock * p.PrecioCompra)
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener stats unificados");
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

        [HttpGet("finanzas-resumen")]
        public async Task<ActionResult<FinanzasResumenDto>> GetFinanzasResumen()
        {
            try
            {
                var inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

                var cuentasCobrar = await _context.CuentasPorCobrar
                    .AsNoTracking()
                    .Where(x => x.Activa)
                    .ToListAsync();

                var cuentasPagar = await _context.CuentasPorPagar
                    .AsNoTracking()
                    .Where(x => x.Activa)
                    .ToListAsync();

                var pagosClienteMes = await _context.PagosCliente
                    .AsNoTracking()
                    .Where(x => x.Activo && x.FechaPago >= inicioMes)
                    .SumAsync(x => (decimal?)x.Monto) ?? 0m;

                var pagosProveedorMes = await _context.PagosProveedor
                    .AsNoTracking()
                    .Where(x => x.Activo && x.FechaPago >= inicioMes)
                    .SumAsync(x => (decimal?)x.Monto) ?? 0m;

                var resumen = new FinanzasResumenDto
                {
                    TotalPorCobrar = cuentasCobrar.Where(x => x.Saldo > 0).Sum(x => x.Saldo),
                    TotalPorPagar = cuentasPagar.Where(x => x.Saldo > 0).Sum(x => x.Saldo),
                    TotalCobradoMes = pagosClienteMes,
                    TotalPagadoMes = pagosProveedorMes,
                    CarteraVencidaClientes = cuentasCobrar.Where(x => x.EstaVencida).Sum(x => x.Saldo),
                    CarteraVencidaProveedores = cuentasPagar.Where(x => x.EstaVencida).Sum(x => x.Saldo),
                    CuentasPorCobrarPendientes = cuentasCobrar.Count(x => x.Saldo > 0),
                    CuentasPorPagarPendientes = cuentasPagar.Count(x => x.Saldo > 0)
                };

                return Ok(resumen);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener resumen financiero");
                return StatusCode(500, "Error interno del servidor");
            }
        }

    }
}
