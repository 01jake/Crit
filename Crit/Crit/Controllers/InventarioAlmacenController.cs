using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Crit.Server.Data;
using Crit.Shared.Models;

namespace Crit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventarioAlmacenController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InventarioAlmacenController> _logger;

        public InventarioAlmacenController(ApplicationDbContext context, ILogger<InventarioAlmacenController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventarioPorAlmacen>>> GetInventario()
        {
            try
            {
                var inventario = await _context.InventarioPorAlmacen
                    .Include(x => x.Producto)
                    .Include(x => x.Almacen)
                    .OrderBy(x => x.Almacen!.Nombre)
                    .ThenBy(x => x.Producto!.Nombre)
                    .ToListAsync();

                return Ok(inventario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inventario por almacen");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("almacen/{almacenId}")]
        public async Task<ActionResult<IEnumerable<InventarioPorAlmacen>>> GetPorAlmacen(int almacenId)
        {
            try
            {
                var inventario = await _context.InventarioPorAlmacen
                    .Include(x => x.Producto)
                    .Include(x => x.Almacen)
                    .Where(x => x.AlmacenId == almacenId)
                    .OrderBy(x => x.Producto!.Nombre)
                    .ToListAsync();

                return Ok(inventario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inventario del almacen {AlmacenId}", almacenId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("producto/{productoId}")]
        public async Task<ActionResult<IEnumerable<InventarioPorAlmacen>>> GetPorProducto(int productoId)
        {
            try
            {
                var inventario = await _context.InventarioPorAlmacen
                    .Include(x => x.Producto)
                    .Include(x => x.Almacen)
                    .Where(x => x.ProductoId == productoId)
                    .OrderBy(x => x.Almacen!.Nombre)
                    .ToListAsync();

                return Ok(inventario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inventario del producto {ProductoId}", productoId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("alertas-minimo")]
        public async Task<ActionResult<IEnumerable<InventarioPorAlmacen>>> GetAlertasStockMinimo()
        {
            try
            {
                var alertas = await _context.InventarioPorAlmacen
                    .Include(x => x.Producto)
                    .Include(x => x.Almacen)
                    .Where(x => x.Stock <= x.StockMinimo)
                    .OrderBy(x => x.Stock)
                    .ToListAsync();

                return Ok(alertas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener alertas de stock minimo");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost]
        public async Task<ActionResult<InventarioPorAlmacen>> CreateInventario([FromBody] InventarioPorAlmacen item)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var productoExiste = await _context.Productos.AnyAsync(p => p.Id == item.ProductoId);
                if (!productoExiste)
                    return BadRequest("El producto no existe");

                var almacenExiste = await _context.Almacenes.AnyAsync(a => a.Id == item.AlmacenId);
                if (!almacenExiste)
                    return BadRequest("El almacen no existe");

                var existeRegistro = await _context.InventarioPorAlmacen
                    .AnyAsync(x => x.ProductoId == item.ProductoId && x.AlmacenId == item.AlmacenId);

                if (existeRegistro)
                    return BadRequest("Ya existe inventario para ese producto en ese almacen");

                _context.InventarioPorAlmacen.Add(item);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPorProducto), new { productoId = item.ProductoId }, item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear inventario por almacen");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInventario(int id, [FromBody] InventarioPorAlmacen item)
        {
            try
            {
                if (id != item.Id)
                    return BadRequest("El ID no coincide");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var inventario = await _context.InventarioPorAlmacen.FindAsync(id);
                if (inventario == null)
                    return NotFound("Registro de inventario no encontrado");

                inventario.Stock = item.Stock;
                inventario.StockMinimo = item.StockMinimo;
                inventario.StockMaximo = item.StockMaximo;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar inventario por almacen {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventario(int id)
        {
            try
            {
                var inventario = await _context.InventarioPorAlmacen.FindAsync(id);
                if (inventario == null)
                    return NotFound("Registro de inventario no encontrado");

                if (inventario.Stock > 0)
                    return BadRequest("No se puede eliminar un registro con stock mayor a cero");

                _context.InventarioPorAlmacen.Remove(inventario);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar inventario por almacen {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}