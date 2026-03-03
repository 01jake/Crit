using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crit.Server.Data;
using Crit.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Crit.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductosController> _logger;

        public ProductosController(ApplicationDbContext context, ILogger<ProductosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/productos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {
            try
            {
                var productos = await _context.Productos
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();
                return Ok(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/productos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Producto>> GetProducto(int id)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);

                if (producto == null)
                {
                    return NotFound($"Producto con ID {id} no encontrado");
                }

                return Ok(producto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener producto {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/productos/activos
        [HttpGet("activos")]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductosActivos()
        {
            try
            {
                var productos = await _context.Productos
                    .Where(p => p.Activo)
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                return Ok(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos activos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/productos/bajo-stock
        [HttpGet("bajo-stock")]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductosBajoStock()
        {
            try
            {
                var productos = await _context.Productos
                    .Where(p => p.Stock <= p.StockMinimo && p.Activo)
                    .OrderBy(p => p.Stock)
                    .ToListAsync();
                return Ok(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos con bajo stock");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/productos
        [HttpPost]
        public async Task<ActionResult<Producto>> CreateProducto([FromBody] Producto producto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validar código único
                var codigoExiste = await _context.Productos
                    .AnyAsync(p => p.Codigo == producto.Codigo);

                if (codigoExiste)
                {
                    return BadRequest("Ya existe un producto con ese código");
                }

                producto.FechaCreacion = DateTime.Now;
                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, producto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear producto");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/productos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProducto(int id, [FromBody] Producto producto)
        {
            try
            {
                if (id != producto.Id)
                {
                    return BadRequest("El ID no coincide");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var productoExiste = await _context.Productos.FindAsync(id);
                if (productoExiste == null)
                {
                    return NotFound($"Producto con ID {id} no encontrado");
                }

                // Validar código único
                var codigoExiste = await _context.Productos
                    .AnyAsync(p => p.Codigo == producto.Codigo && p.Id != id);

                if (codigoExiste)
                {
                    return BadRequest("Ya existe otro producto con ese código");
                }

                // Actualizar propiedades
                productoExiste.Codigo = producto.Codigo;
                productoExiste.Nombre = producto.Nombre;
                productoExiste.Descripcion = producto.Descripcion;
                productoExiste.PrecioCompra = producto.PrecioCompra;
                productoExiste.PrecioVenta = producto.PrecioVenta;
                productoExiste.Stock = producto.Stock;
                productoExiste.StockMinimo = producto.StockMinimo;
                productoExiste.Categoria = producto.Categoria;
                productoExiste.Unidad = producto.Unidad;
                productoExiste.Activo = producto.Activo;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar producto {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/productos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                {
                    return NotFound($"Producto con ID {id} no encontrado");
                }

                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar producto {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/productos/5/stock
        [HttpPut("{id}/stock")]
        public async Task<IActionResult> ActualizarStock(int id, [FromBody] int cantidad)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                {
                    return NotFound($"Producto con ID {id} no encontrado");
                }

                producto.Stock += cantidad;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar stock del producto {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/productos/count
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetProductosCount()
        {
            var count = await _context.Productos.CountAsync(p => p.Activo);
            return Ok(count);
        }
    }
}
