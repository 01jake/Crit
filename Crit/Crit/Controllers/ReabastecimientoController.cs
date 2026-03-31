using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Crit.Server.Data;
using Crit.Shared.Models;
using Crit.Shared.DTOs;

namespace Crit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReabastecimientoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReabastecimientoController> _logger;

        public ReabastecimientoController(ApplicationDbContext context, ILogger<ReabastecimientoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrdenReabastecimiento>>> GetOrdenes()
        {
            try
            {
                var ordenes = await _context.OrdenesReabastecimiento
                    .Include(x => x.Producto)
                    .Include(x => x.Almacen)
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
                var ordenes = await _context.OrdenesReabastecimiento
                    .Include(x => x.Producto)
                    .Include(x => x.Almacen)
                    .Where(x => x.Estado == "Pendiente" || x.Estado == "Solicitada" || x.Estado == "EnProceso")
                    .OrderByDescending(x => x.Fecha)
                    .ToListAsync();

                return Ok(ordenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ordenes pendientes de reabastecimiento");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("generar-alertas")]
        public async Task<ActionResult> GenerarAlertas()
        {
            try
            {
                var inventarioEnAlerta = await _context.InventarioPorAlmacen
                    .Include(x => x.Producto)
                    .Include(x => x.Almacen)
                    .Where(x => x.Stock <= x.StockMinimo)
                    .ToListAsync();

                var creadas = 0;

                foreach (var item in inventarioEnAlerta)
                {
                    var yaExiste = await _context.OrdenesReabastecimiento.AnyAsync(x =>
                        x.ProductoId == item.ProductoId &&
                        x.AlmacenId == item.AlmacenId &&
                        (x.Estado == "Pendiente" || x.Estado == "Solicitada" || x.Estado == "EnProceso"));

                    if (yaExiste)
                        continue;

                    var sugerida = item.StockMinimo > item.Stock
                        ? item.StockMinimo - item.Stock
                        : 0;

                    if (sugerida <= 0)
                        sugerida = 1;

                    var orden = new OrdenReabastecimiento
                    {
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

                return Ok(new
                {
                    message = "Alertas generadas correctamente",
                    total = creadas
                });
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
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (orden.CantidadSugerida <= 0)
                    return BadRequest("La cantidad sugerida debe ser mayor a cero");

                var productoExiste = await _context.Productos.AnyAsync(x => x.Id == orden.ProductoId);
                if (!productoExiste)
                    return BadRequest("El producto no existe");

                var almacenExiste = await _context.Almacenes.AnyAsync(x => x.Id == orden.AlmacenId && x.Activo);
                if (!almacenExiste)
                    return BadRequest("El almacen no existe o esta inactivo");

                orden.Fecha = DateTime.Now;

                if (string.IsNullOrWhiteSpace(orden.Estado))
                    orden.Estado = "Pendiente";

                _context.OrdenesReabastecimiento.Add(orden);
                await _context.SaveChangesAsync();

                var creada = await _context.OrdenesReabastecimiento
                    .Include(x => x.Producto)
                    .Include(x => x.Almacen)
                    .FirstOrDefaultAsync(x => x.Id == orden.Id);

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
                var orden = await _context.OrdenesReabastecimiento.FindAsync(id);
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
                _logger.LogError(ex, "Error al solicitar orden de reabastecimiento {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("{id}/completar")]
        public async Task<IActionResult> Completar(int id)
        {
            try
            {
                var orden = await _context.OrdenesReabastecimiento.FindAsync(id);
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
                _logger.LogError(ex, "Error al completar orden de reabastecimiento {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("{id}/cancelar")]
        public async Task<IActionResult> Cancelar(int id)
        {
            try
            {
                var orden = await _context.OrdenesReabastecimiento.FindAsync(id);
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
                _logger.LogError(ex, "Error al cancelar orden de reabastecimiento {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }
        [HttpPost("{id}/crear-compra")]
        public async Task<IActionResult> CrearCompraDesdeOrden(int id, [FromBody] CrearCompraDesdeReabastecimientoDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var orden = await _context.OrdenesReabastecimiento
                    .Include(x => x.Producto)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (orden == null)
                    return NotFound("Orden no encontrada");

                if (orden.Estado == "Completada" || orden.Estado == "Cancelada")
                    return BadRequest("La orden ya no puede procesarse");

                if (dto.Cantidad <= 0)
                    return BadRequest("La cantidad debe ser mayor a cero");

                var proveedor = await _context.Proveedores.FindAsync(dto.ProveedorId);
                if (proveedor == null)
                    return BadRequest("Proveedor no encontrado");

                var compra = new Compra
                {
                    Fecha = DateTime.Now,
                    ProveedorId = dto.ProveedorId,
                    AlmacenId = dto.AlmacenId,
                    SerieFactura = dto.SerieFactura,
                    FolioFactura = dto.FolioFactura,
                    RFCProveedor = string.IsNullOrWhiteSpace(dto.RFCProveedor) ? proveedor.RFC : dto.RFCProveedor,
                    FechaFactura = dto.FechaFactura,
                    EsCredito = dto.EsCredito,
                    DiasCredito = dto.DiasCredito,
                    Detalles = new List<DetalleCompra>
            {
                new DetalleCompra
                {
                    ProductoId = orden.ProductoId,
                    Cantidad = (int)dto.Cantidad,
                    PrecioUnitario = dto.PrecioUnitario,
                    Subtotal = dto.Cantidad * dto.PrecioUnitario
                }
            }
                };

                _context.Compra.Add(compra);
                await _context.SaveChangesAsync();

                orden.Estado = "Solicitada";
                orden.TipoSugerido = "Compra";
                orden.CompraId = compra.Id;
                orden.Observaciones = $"Orden vinculada a compra #{compra.Id}";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Compra creada desde orden de reabastecimiento",
                    compraId = compra.Id
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear compra desde orden {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }
        [HttpPost("{id}/crear-traspaso")]
        public async Task<IActionResult> CrearTraspasoDesdeOrden(int id, [FromBody] CrearTraspasoDesdeReabastecimientoDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var orden = await _context.OrdenesReabastecimiento
                    .Include(x => x.Producto)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (orden == null)
                    return NotFound("Orden no encontrada");

                if (orden.Estado == "Completada" || orden.Estado == "Cancelada")
                    return BadRequest("La orden ya no puede procesarse");

                if (dto.Cantidad <= 0)
                    return BadRequest("La cantidad debe ser mayor a cero");

                if (dto.AlmacenOrigenId == dto.AlmacenDestinoId)
                    return BadRequest("El almacén origen y destino no pueden ser el mismo");

                var inventarioOrigen = await _context.InventarioPorAlmacen
                    .FirstOrDefaultAsync(x => x.ProductoId == dto.ProductoId && x.AlmacenId == dto.AlmacenOrigenId);

                if (inventarioOrigen == null || inventarioOrigen.Stock < dto.Cantidad)
                    return BadRequest("No hay stock suficiente en el almacén origen");

                var traspaso = new TraspasoAlmacen
                {
                    Fecha = DateTime.Now,
                    AlmacenOrigenId = dto.AlmacenOrigenId,
                    AlmacenDestinoId = dto.AlmacenDestinoId,
                    ProductoId = dto.ProductoId,
                    Cantidad = dto.Cantidad,
                    Estado = "Completado",
                    Observaciones = dto.Observaciones
                };

                _context.TraspasosAlmacen.Add(traspaso);
                await _context.SaveChangesAsync();

                orden.Estado = "Solicitada";
                orden.TipoSugerido = "Traspaso";
                orden.TraspasoAlmacenId = traspaso.Id;
                orden.Observaciones = $"Orden vinculada a traspaso #{traspaso.Id}";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Traspaso creado desde orden de reabastecimiento",
                    traspasoId = traspaso.Id
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear traspaso desde orden {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }
        [HttpPost("{id}/vincular-compra/{compraId}")]
        public async Task<IActionResult> VincularCompra(int id, int compraId)
        {
            try
            {
                var orden = await _context.OrdenesReabastecimiento.FindAsync(id);
                if (orden == null)
                    return NotFound("Orden no encontrada");

                var compra = await _context.Compra.FindAsync(compraId);
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
                var orden = await _context.OrdenesReabastecimiento.FindAsync(id);
                if (orden == null)
                    return NotFound("Orden no encontrada");

                var traspaso = await _context.TraspasosAlmacen.FindAsync(traspasoId);
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
                var orden = await _context.OrdenesReabastecimiento.FindAsync(id);
                if (orden == null)
                    return NotFound("Orden no encontrada");

                var compra = await _context.Compra.FindAsync(compraId);
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
                var orden = await _context.OrdenesReabastecimiento.FindAsync(id);
                if (orden == null)
                    return NotFound("Orden no encontrada");

                var traspaso = await _context.TraspasosAlmacen.FindAsync(traspasoId);
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