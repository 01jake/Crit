using Crit.Server.Data;
using Crit.Shared.DTOs;
using Crit.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KardexController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<KardexController> _logger;
        private readonly IEmpresaProvider _empresaProvider;

        public KardexController(
            ApplicationDbContext context,
            ILogger<KardexController> logger,
            IEmpresaProvider empresaProvider)
        {
            _context = context;
            _logger = logger;
            _empresaProvider = empresaProvider;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<KardexMovimientoDto>>> GetKardex(
            [FromQuery] int? productoId,
            [FromQuery] int? almacenId,
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var query = _context.MovimientosInventario
                    .Include(x => x.Producto)
                    .Include(x => x.Almacen)
                    .Where(x => x.EmpresaId == empresaId)
                    .AsQueryable();

                if (productoId.HasValue)
                    query = query.Where(x => x.ProductoId == productoId.Value);

                if (almacenId.HasValue)
                    query = query.Where(x => x.AlmacenId == almacenId.Value);

                if (fechaInicio.HasValue)
                    query = query.Where(x => x.Fecha >= fechaInicio.Value.Date);

                if (fechaFin.HasValue)
                {
                    var fin = fechaFin.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(x => x.Fecha <= fin);
                }

                var kardex = await query
                    .OrderByDescending(x => x.Fecha)
                    .Select(x => new KardexMovimientoDto
                    {
                        Fecha = x.Fecha,
                        Producto = x.Producto != null ? x.Producto.Nombre : string.Empty,
                        CodigoProducto = x.Producto != null ? x.Producto.Codigo : null,
                        Almacen = x.Almacen != null ? x.Almacen.Nombre : string.Empty,
                        TipoMovimiento = x.TipoMovimiento,
                        Cantidad = x.Cantidad,
                        StockAnterior = x.StockAnterior,
                        StockNuevo = x.StockNuevo,
                        Referencia = x.Referencia,
                        Observaciones = x.Observaciones
                    })
                    .ToListAsync();

                return Ok(kardex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener kardex");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
