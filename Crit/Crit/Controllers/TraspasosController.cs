using Crit.Server.Data;
using Crit.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TraspasosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TraspasosController> _logger;
        private readonly IEmpresaProvider _empresaProvider;

        public TraspasosController(
            ApplicationDbContext context,
            ILogger<TraspasosController> logger,
            IEmpresaProvider empresaProvider)
        {
            _context = context;
            _logger = logger;
            _empresaProvider = empresaProvider;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TraspasoAlmacen>>> GetTraspasos()
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var traspasos = await _context.TraspasosAlmacen
                    .Include(x => x.AlmacenOrigen)
                    .Include(x => x.AlmacenDestino)
                    .Include(x => x.Producto)
                    .Where(x => x.EmpresaId == empresaId)
                    .OrderByDescending(x => x.Fecha)
                    .ToListAsync();

                return Ok(traspasos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener traspasos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TraspasoAlmacen>> GetTraspaso(int id)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var traspaso = await _context.TraspasosAlmacen
                    .Include(x => x.AlmacenOrigen)
                    .Include(x => x.AlmacenDestino)
                    .Include(x => x.Producto)
                    .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);

                if (traspaso == null)
                    return NotFound($"Traspaso con ID {id} no encontrado");

                return Ok(traspaso);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener traspaso {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TraspasoAlmacen>> CrearTraspaso([FromBody] TraspasoAlmacen traspaso)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (traspaso.AlmacenOrigenId == traspaso.AlmacenDestinoId)
                    return BadRequest("El almacén origen y destino no pueden ser el mismo");

                if (traspaso.Cantidad <= 0)
                    return BadRequest("La cantidad debe ser mayor a cero");

                var almacenOrigen = await _context.Almacenes
                    .FirstOrDefaultAsync(a => a.Id == traspaso.AlmacenOrigenId && a.Activo && a.EmpresaId == empresaId);

                var almacenDestino = await _context.Almacenes
                    .FirstOrDefaultAsync(a => a.Id == traspaso.AlmacenDestinoId && a.Activo && a.EmpresaId == empresaId);

                if (almacenOrigen == null)
                    return BadRequest("El almacén origen no existe o está inactivo");

                if (almacenDestino == null)
                    return BadRequest("El almacén destino no existe o está inactivo");

                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p => p.Id == traspaso.ProductoId && p.EmpresaId == empresaId);

                if (producto == null)
                    return BadRequest("El producto no existe");

                var inventarioOrigen = await _context.InventarioPorAlmacen
                    .FirstOrDefaultAsync(x => x.ProductoId == traspaso.ProductoId &&
                                              x.AlmacenId == traspaso.AlmacenOrigenId &&
                                              x.EmpresaId == empresaId);

                if (inventarioOrigen == null)
                    return BadRequest("El producto no tiene inventario en el almacén origen");

                if (inventarioOrigen.Stock < traspaso.Cantidad)
                    return BadRequest($"Stock insuficiente en almacén origen. Disponible: {inventarioOrigen.Stock}");

                var inventarioDestino = await _context.InventarioPorAlmacen
                    .FirstOrDefaultAsync(x => x.ProductoId == traspaso.ProductoId &&
                                              x.AlmacenId == traspaso.AlmacenDestinoId &&
                                              x.EmpresaId == empresaId);

                if (inventarioDestino == null)
                {
                    inventarioDestino = new InventarioPorAlmacen
                    {
                        EmpresaId = empresaId,
                        ProductoId = traspaso.ProductoId,
                        AlmacenId = traspaso.AlmacenDestinoId,
                        Stock = 0,
                        StockMinimo = producto.StockMinimo,
                        StockMaximo = 0
                    };

                    _context.InventarioPorAlmacen.Add(inventarioDestino);
                    await _context.SaveChangesAsync();
                }

                var stockAnteriorOrigen = inventarioOrigen.Stock;
                var stockAnteriorDestino = inventarioDestino.Stock;

                inventarioOrigen.Stock -= traspaso.Cantidad;
                inventarioDestino.Stock += traspaso.Cantidad;

                producto.Stock = (int)await _context.InventarioPorAlmacen
                    .Where(x => x.EmpresaId == empresaId && x.ProductoId == traspaso.ProductoId)
                    .SumAsync(x => x.Stock);

                traspaso.EmpresaId = empresaId;
                traspaso.Fecha = DateTime.Now;
                traspaso.Estado = "Completado";

                _context.TraspasosAlmacen.Add(traspaso);
                await _context.SaveChangesAsync();

                var movimientoSalida = new MovimientoInventario
                {
                    EmpresaId = empresaId,
                    Fecha = traspaso.Fecha,
                    ProductoId = traspaso.ProductoId,
                    AlmacenId = traspaso.AlmacenOrigenId,
                    TipoMovimiento = "TraspasoSalida",
                    Cantidad = traspaso.Cantidad,
                    StockAnterior = stockAnteriorOrigen,
                    StockNuevo = inventarioOrigen.Stock,
                    Referencia = $"TRASPASO-{traspaso.Id}",
                    Observaciones = $"Salida por traspaso a almacén {almacenDestino.Nombre}",
                    TraspasoId = traspaso.Id
                };

                var movimientoEntrada = new MovimientoInventario
                {
                    EmpresaId = empresaId,
                    Fecha = traspaso.Fecha,
                    ProductoId = traspaso.ProductoId,
                    AlmacenId = traspaso.AlmacenDestinoId,
                    TipoMovimiento = "TraspasoEntrada",
                    Cantidad = traspaso.Cantidad,
                    StockAnterior = stockAnteriorDestino,
                    StockNuevo = inventarioDestino.Stock,
                    Referencia = $"TRASPASO-{traspaso.Id}",
                    Observaciones = $"Entrada por traspaso desde almacén {almacenOrigen.Nombre}",
                    TraspasoId = traspaso.Id
                };

                _context.MovimientosInventario.Add(movimientoSalida);
                _context.MovimientosInventario.Add(movimientoEntrada);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var traspasoCreado = await _context.TraspasosAlmacen
                    .Include(x => x.AlmacenOrigen)
                    .Include(x => x.AlmacenDestino)
                    .Include(x => x.Producto)
                    .FirstOrDefaultAsync(x => x.Id == traspaso.Id && x.EmpresaId == empresaId);

                return CreatedAtAction(nameof(GetTraspaso), new { id = traspaso.Id }, traspasoCreado);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear traspaso");
                return StatusCode(500, "Error interno del servidor");
            }
        }


        [HttpPost("{id}/cancelar")]
        public async Task<IActionResult> CancelarTraspaso(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var traspaso = await _context.TraspasosAlmacen
                    .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);

                if (traspaso == null)
                    return NotFound("Traspaso no encontrado");

                if (traspaso.Estado == "Cancelado")
                    return BadRequest("El traspaso ya está cancelado");

                var inventarioOrigen = await _context.InventarioPorAlmacen
                    .FirstOrDefaultAsync(x => x.ProductoId == traspaso.ProductoId &&
                                              x.AlmacenId == traspaso.AlmacenOrigenId &&
                                              x.EmpresaId == empresaId);

                var inventarioDestino = await _context.InventarioPorAlmacen
                    .FirstOrDefaultAsync(x => x.ProductoId == traspaso.ProductoId &&
                                              x.AlmacenId == traspaso.AlmacenDestinoId &&
                                              x.EmpresaId == empresaId);

                if (inventarioOrigen == null || inventarioDestino == null)
                    return BadRequest("No se encontró inventario relacionado al traspaso");

                if (inventarioDestino.Stock < traspaso.Cantidad)
                    return BadRequest("No se puede cancelar porque el almacén destino ya no tiene stock suficiente para revertir");

                var stockAnteriorOrigen = inventarioOrigen.Stock;
                var stockAnteriorDestino = inventarioDestino.Stock;

                inventarioDestino.Stock -= traspaso.Cantidad;
                inventarioOrigen.Stock += traspaso.Cantidad;

                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p => p.Id == traspaso.ProductoId && p.EmpresaId == empresaId);

                if (producto != null)
                {
                    producto.Stock = (int)await _context.InventarioPorAlmacen
                        .Where(x => x.EmpresaId == empresaId && x.ProductoId == traspaso.ProductoId)
                        .SumAsync(x => x.Stock);
                }

                traspaso.Estado = "Cancelado";

                var movimientoSalidaCancelacion = new MovimientoInventario
                {
                    EmpresaId = empresaId,
                    Fecha = DateTime.Now,
                    ProductoId = traspaso.ProductoId,
                    AlmacenId = traspaso.AlmacenDestinoId,
                    TipoMovimiento = "TraspasoSalida",
                    Cantidad = traspaso.Cantidad,
                    StockAnterior = stockAnteriorDestino,
                    StockNuevo = inventarioDestino.Stock,
                    Referencia = $"TRASPASO-CANCEL-{traspaso.Id}",
                    Observaciones = "Salida por cancelación de traspaso",
                    TraspasoId = traspaso.Id
                };

                var movimientoEntradaCancelacion = new MovimientoInventario
                {
                    EmpresaId = empresaId,
                    Fecha = DateTime.Now,
                    ProductoId = traspaso.ProductoId,
                    AlmacenId = traspaso.AlmacenOrigenId,
                    TipoMovimiento = "TraspasoEntrada",
                    Cantidad = traspaso.Cantidad,
                    StockAnterior = stockAnteriorOrigen,
                    StockNuevo = inventarioOrigen.Stock,
                    Referencia = $"TRASPASO-CANCEL-{traspaso.Id}",
                    Observaciones = "Entrada por cancelación de traspaso",
                    TraspasoId = traspaso.Id
                };

                _context.MovimientosInventario.Add(movimientoSalidaCancelacion);
                _context.MovimientosInventario.Add(movimientoEntradaCancelacion);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Traspaso cancelado correctamente" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al cancelar traspaso {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

    }
}
