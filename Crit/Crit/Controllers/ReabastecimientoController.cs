using Crit.Server.Data;
using Crit.Shared.DTOs;
using Crit.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReabastecimientoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReabastecimientoController> _logger;
        private readonly IEmpresaProvider _empresaProvider;

        public ReabastecimientoController(
            ApplicationDbContext context,
            ILogger<ReabastecimientoController> logger,
            IEmpresaProvider empresaProvider)
        {
            _context = context;
            _logger = logger;
            _empresaProvider = empresaProvider;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrdenReabastecimiento>>> GetOrdenes()
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();
                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var ordenes = await _context.OrdenesReabastecimiento
                    .Include(x => x.Producto)
                    .Include(x => x.Almacen)
                    .Where(x => x.EmpresaId == empresaId)
                    .OrderByDescending(x => x.Fecha)
                    .ToListAsync();

                return Ok(ordenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ordenes de reabastecimiento");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("pendientes")]
        public async Task<ActionResult<IEnumerable<OrdenReabastecimiento>>> GetPendientes()
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();
                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var ordenes = await _context.OrdenesReabastecimiento
                    .Include(x => x.Producto)
                    .Include(x => x.Almacen)
                    .Where(x => x.EmpresaId == empresaId &&
                                (x.Estado == "Pendiente" || x.Estado == "Solicitada" || x.Estado == "EnProceso"))
                    .OrderByDescending(x => x.Fecha)
                    .ToListAsync();

                return Ok(ordenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ordenes pendientes");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("generar-alertas")]
        public async Task<ActionResult> GenerarAlertas()
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();
                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var inventarioEnAlerta = await _context.InventarioPorAlmacen
                    .Include(x => x.Producto)
                    .Include(x => x.Almacen)
                    .Where(x => x.EmpresaId == empresaId && x.Stock <= x.StockMinimo)
                    .ToListAsync();

                var creadas = 0;

                foreach (var item in inventarioEnAlerta)
                {
                    var yaExiste = await _context.OrdenesReabastecimiento.AnyAsync(x =>
                        x.EmpresaId == empresaId &&
                        x.ProductoId == item.ProductoId &&
                        x.AlmacenId == item.AlmacenId &&
                        (x.Estado == "Pendiente" || x.Estado == "Solicitada" || x.Estado == "EnProceso"));

                    if (yaExiste)
                        continue;

                    var sugerida = item.StockMinimo > item.Stock
                        ? item.StockMinimo - item.Stock
                        : 1;

                    var orden = new OrdenReabastecimiento
                    {
                        EmpresaId = empresaId,
                        Fecha = DateTime.Now,
                        ProductoId = item.ProductoId,
                        AlmacenId = item.AlmacenId,
                        StockActual = item.Stock,
                        StockMinimo = item.StockMinimo,
                        CantidadSugerida = sugerida,
                        Estado = "Pendiente",
                        TipoSugerido = "Compra",
                        Observaciones = "Generada automaticamente por stock minimo"
                    };

                    _context.OrdenesReabastecimiento.Add(orden);
                    creadas++;
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Alertas generadas correctamente", total = creadas });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar alertas de reabastecimiento");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost]
        public async Task<ActionResult<OrdenReabastecimiento>> CreateOrden([FromBody] OrdenReabastecimiento orden)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();
                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (orden.CantidadSugerida <= 0)
                    return BadRequest("La cantidad sugerida debe ser mayor a cero");

                var productoExiste = await _context.Productos.AnyAsync(x => x.Id == orden.ProductoId && x.EmpresaId == empresaId);
                if (!productoExiste)
                    return BadRequest("El producto no existe");

                var almacenExiste = await _context.Almacenes.AnyAsync(x => x.Id == orden.AlmacenId && x.EmpresaId == empresaId && x.Activo);
                if (!almacenExiste)
                    return BadRequest("El almacen no existe o esta inactivo");

                orden.EmpresaId = empresaId;
                orden.Fecha = DateTime.Now;

                if (string.IsNullOrWhiteSpace(orden.Estado))
                    orden.Estado = "Pendiente";

                _context.OrdenesReabastecimiento.Add(orden);
                await _context.SaveChangesAsync();

                var creada = await _context.OrdenesReabastecimiento
                    .Include(x => x.Producto)
                    .Include(x => x.Almacen)
                    .FirstOrDefaultAsync(x => x.Id == orden.Id && x.EmpresaId == empresaId);

                return CreatedAtAction(nameof(GetOrdenes), new { id = orden.Id }, creada);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear orden de reabastecimiento");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("{id}/solicitar")]
        public async Task<IActionResult> Solicitar(int id)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();
                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var orden = await _context.OrdenesReabastecimiento
                    .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);

                if (orden == null)
                    return NotFound("Orden no encontrada");

                if (orden.Estado == "Completada" || orden.Estado == "Cancelada")
                    return BadRequest("No se puede cambiar el estado de esta orden");

                orden.Estado = "Solicitada";
                await _context.SaveChangesAsync();

                return Ok(new { message = "Orden marcada como solicitada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al solicitar orden {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("{id}/completar")]
        public async Task<IActionResult> Completar(int id)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();
                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var orden = await _context.OrdenesReabastecimiento
                    .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);

                if (orden == null)
                    return NotFound("Orden no encontrada");

                if (orden.Estado == "Cancelada")
                    return BadRequest("No se puede completar una orden cancelada");

                orden.Estado = "Completada";
                await _context.SaveChangesAsync();

                return Ok(new { message = "Orden marcada como completada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar orden {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("{id}/cancelar")]
        public async Task<IActionResult> Cancelar(int id)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();
                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var orden = await _context.OrdenesReabastecimiento
                    .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);

                if (orden == null)
                    return NotFound("Orden no encontrada");

                if (orden.Estado == "Completada")
                    return BadRequest("No se puede cancelar una orden completada");

                orden.Estado = "Cancelada";
                await _context.SaveChangesAsync();

                return Ok(new { message = "Orden cancelada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar orden {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("{id}/vincular-compra/{compraId}")]
        public async Task<IActionResult> VincularCompra(int id, int compraId)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();
                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var orden = await _context.OrdenesReabastecimiento
                    .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);

                if (orden == null)
                    return NotFound("Orden no encontrada");

                var compra = await _context.Compra
                    .FirstOrDefaultAsync(x => x.Id == compraId && x.EmpresaId == empresaId);

                if (compra == null)
                    return BadRequest("Compra no encontrada");

                orden.CompraId = compraId;
                orden.TipoSugerido = "Compra";
                orden.Estado = "Solicitada";
                orden.Observaciones = $"Orden vinculada a compra #{compraId}";

                await _context.SaveChangesAsync();

                return Ok(new { message = "Orden vinculada a compra correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al vincular compra {CompraId} con orden {Id}", compraId, id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("{id}/vincular-traspaso/{traspasoId}")]
        public async Task<IActionResult> VincularTraspaso(int id, int traspasoId)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();
                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var orden = await _context.OrdenesReabastecimiento
                    .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);

                if (orden == null)
                    return NotFound("Orden no encontrada");

                var traspaso = await _context.TraspasosAlmacen
                    .FirstOrDefaultAsync(x => x.Id == traspasoId && x.EmpresaId == empresaId);

                if (traspaso == null)
                    return BadRequest("Traspaso no encontrado");

                orden.TraspasoAlmacenId = traspasoId;
                orden.TipoSugerido = "Traspaso";
                orden.Estado = "Solicitada";
                orden.Observaciones = $"Orden vinculada a traspaso #{traspasoId}";

                await _context.SaveChangesAsync();

                return Ok(new { message = "Orden vinculada a traspaso correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al vincular traspaso {TraspasoId} con orden {Id}", traspasoId, id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("{id}/completar-desde-compra/{compraId}")]
        public async Task<IActionResult> CompletarDesdeCompra(int id, int compraId)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();
                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var orden = await _context.OrdenesReabastecimiento
                    .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);

                if (orden == null)
                    return NotFound("Orden no encontrada");

                var compra = await _context.Compra
                    .FirstOrDefaultAsync(x => x.Id == compraId && x.EmpresaId == empresaId);

                if (compra == null)
                    return BadRequest("Compra no encontrada");

                orden.CompraId = compraId;
                orden.TipoSugerido = "Compra";
                orden.Estado = "Completada";
                orden.Observaciones = $"Orden completada desde compra #{compraId}";

                await _context.SaveChangesAsync();

                return Ok(new { message = "Orden completada desde compra correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar orden {Id} desde compra {CompraId}", id, compraId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("{id}/completar-desde-traspaso/{traspasoId}")]
        public async Task<IActionResult> CompletarDesdeTraspaso(int id, int traspasoId)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();
                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var orden = await _context.OrdenesReabastecimiento
                    .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);

                if (orden == null)
                    return NotFound("Orden no encontrada");

                var traspaso = await _context.TraspasosAlmacen
                    .FirstOrDefaultAsync(x => x.Id == traspasoId && x.EmpresaId == empresaId);

                if (traspaso == null)
                    return BadRequest("Traspaso no encontrado");

                orden.TraspasoAlmacenId = traspasoId;
                orden.TipoSugerido = "Traspaso";
                orden.Estado = "Completada";
                orden.Observaciones = $"Orden completada desde traspaso #{traspasoId}";

                await _context.SaveChangesAsync();

                return Ok(new { message = "Orden completada desde traspaso correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar orden {Id} desde traspaso {TraspasoId}", id, traspasoId);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
