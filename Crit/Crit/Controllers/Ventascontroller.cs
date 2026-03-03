using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crit.Server.Data;
using Crit.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VentasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VentasController> _logger;

        public VentasController(ApplicationDbContext context, ILogger<VentasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/ventas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Venta>>> GetVentas()
        {
            try
            {
                var ventas = await _context.Ventas
                    .Include(v => v.Cliente)
                    .OrderByDescending(v => v.Fecha)
                    .ToListAsync();
                return Ok(ventas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/ventas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Venta>> GetVenta(int id)
        {
            try
            {
                var venta = await _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (venta == null)
                {
                    return NotFound($"Venta con ID {id} no encontrada");
                }

                return Ok(venta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener venta {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/ventas/cliente/5
        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<Venta>>> GetVentasPorCliente(int clienteId)
        {
            try
            {
                var ventas = await _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.Detalles)
                    .Where(v => v.ClienteId == clienteId)
                    .OrderByDescending(v => v.Fecha)
                    .ToListAsync();
                return Ok(ventas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas del cliente {ClienteId}", clienteId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/ventas/recientes?cantidad=10
        [HttpGet("recientes")]
        public async Task<ActionResult<IEnumerable<Venta>>> GetVentasRecientes([FromQuery] int cantidad = 10)
        {
            try
            {
                var ventas = await _context.Ventas
                    .Include(v => v.Cliente)
                    .OrderByDescending(v => v.Fecha)
                    .Take(cantidad)
                    .ToListAsync();
                return Ok(ventas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas recientes");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/ventas
        [HttpPost]
        public async Task<ActionResult<Venta>> CreateVenta([FromBody] Venta venta)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("=== INICIO: Creando venta ===");

                // Validaciones
                if (venta.ClienteId == 0)
                {
                    _logger.LogWarning("ClienteId es 0");
                    return BadRequest("Debe seleccionar un cliente");
                }

                if (venta.Detalles == null || !venta.Detalles.Any())
                {
                    _logger.LogWarning("No hay detalles en la venta");
                    return BadRequest("La venta debe tener al menos un producto");
                }

                _logger.LogInformation($"Venta con {venta.Detalles.Count} productos");

                // ✅ Limpiar TODAS las navegaciones que vienen del cliente
                venta.Cliente = null;
                venta.Id = 0;

                foreach (var detalle in venta.Detalles)
                {
                    detalle.Venta = null;
                    detalle.Producto = null;
                    detalle.VentaId = 0;
                    detalle.Id = 0;

                    _logger.LogInformation($"Detalle: ProductoId={detalle.ProductoId}, Cantidad={detalle.Cantidad}");
                }

                // Generar número de venta
                var ultimaVenta = await _context.Ventas
                    .AsNoTracking()
                    .OrderByDescending(v => v.Id)
                    .FirstOrDefaultAsync();

                var numero = ultimaVenta != null ? ultimaVenta.Id + 1 : 1;
                venta.NumeroVenta = $"V-{DateTime.Now:yyyyMMdd}-{numero:D6}";
                venta.Fecha = DateTime.Now;

                _logger.LogInformation($"Número de venta generado: {venta.NumeroVenta}");

                // Validar y descontar stock
                foreach (var detalle in venta.Detalles)
                {
                    var producto = await _context.Productos.FindAsync(detalle.ProductoId);

                    if (producto == null)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError($"Producto {detalle.ProductoId} no encontrado");
                        return BadRequest($"Producto con ID {detalle.ProductoId} no encontrado");
                    }

                    if (producto.Stock < detalle.Cantidad)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError($"Stock insuficiente para {producto.Nombre}");
                        return BadRequest($"Stock insuficiente para {producto.Nombre}. Disponible: {producto.Stock}");
                    }

                    _logger.LogInformation($"Descontando {detalle.Cantidad} de {producto.Nombre}. Stock actual: {producto.Stock}");

                    producto.Stock -= detalle.Cantidad;
                }

                // Agregar la venta
                _context.Ventas.Add(venta);

                _logger.LogInformation("Guardando cambios en la base de datos...");

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"✅ Venta creada exitosamente con ID: {venta.Id}");

                // ✅ Retornar solo datos básicos sin navegaciones complejas
                return Ok(new
                {
                    Id = venta.Id,
                    NumeroVenta = venta.NumeroVenta,
                    ClienteId = venta.ClienteId,
                    Fecha = venta.Fecha,
                    Subtotal = venta.Subtotal,
                    Descuento = venta.Descuento,
                    IVA = venta.IVA,
                    Total = venta.Total,
                    Estado = venta.Estado,
                    Notas = venta.Notas
                });
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                _logger.LogError(dbEx, "Error de base de datos al crear venta");
                _logger.LogError($"Inner: {dbEx.InnerException?.Message}");
                return StatusCode(500, new
                {
                    error = "Error de base de datos",
                    message = dbEx.InnerException?.Message ?? dbEx.Message
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear venta");
                _logger.LogError($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    error = "Error interno",
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        // GET: api/ventas/fecha
        [HttpGet("fecha")]
        public async Task<ActionResult<IEnumerable<Venta>>> GetVentasPorFecha(
            [FromQuery] DateTime desde,
            [FromQuery] DateTime hasta)
        {
            try
            {
                var ventas = await _context.Ventas
                    .Include(v => v.Cliente)
                    .Where(v => v.Fecha >= desde && v.Fecha <= hasta)
                    .OrderByDescending(v => v.Fecha)
                    .ToListAsync();

                return Ok(ventas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas por fecha");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/ventas/total-mes?mes=1&año=2024
        [HttpGet("total-mes")]
        public async Task<ActionResult<decimal>> GetTotalVentasMes([FromQuery] int mes, [FromQuery] int año)
        {
            try
            {
                var total = await _context.Ventas
                    .Where(v => v.Fecha.Month == mes && v.Fecha.Year == año && v.Estado == "Completada")
                    .SumAsync(v => v.Total);
                return Ok(total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener total de ventas del mes");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}