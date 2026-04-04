using Crit.Server.Data;
using Crit.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductosController> _logger;
        private readonly IEmpresaProvider _empresaProvider;

        public ProductosController(
            ApplicationDbContext context,
            ILogger<ProductosController> logger,
            IEmpresaProvider empresaProvider)
        {
            _context = context;
            _logger = logger;
            _empresaProvider = empresaProvider;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var productos = await _context.Productos
                    .Where(p => p.EmpresaId == empresaId)
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

        [HttpGet("{id}")]
        public async Task<ActionResult<Producto>> GetProducto(int id)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p => p.Id == id && p.EmpresaId == empresaId);

                if (producto == null)
                    return NotFound($"Producto con ID {id} no encontrado");

                return Ok(producto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener producto {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("activos")]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductosActivos()
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var productos = await _context.Productos
                    .Where(p => p.EmpresaId == empresaId && p.Activo)
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

        [HttpPost]
        public async Task<ActionResult<Producto>> CreateProducto([FromBody] Producto producto)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var codigoExiste = await _context.Productos
                    .AnyAsync(p => p.Codigo == producto.Codigo && p.EmpresaId == empresaId);

                if (codigoExiste)
                    return BadRequest("Ya existe un producto con ese código en esta empresa");

                producto.EmpresaId = empresaId;

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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProducto(int id, [FromBody] Producto producto)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                if (id != producto.Id)
                    return BadRequest("El ID no coincide");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var productoExiste = await _context.Productos
                    .FirstOrDefaultAsync(p => p.Id == id && p.EmpresaId == empresaId);

                if (productoExiste == null)
                    return NotFound($"Producto con ID {id} no encontrado");

                var codigoExiste = await _context.Productos
                    .AnyAsync(p => p.Codigo == producto.Codigo && p.Id != id && p.EmpresaId == empresaId);

                if (codigoExiste)
                    return BadRequest("Ya existe otro producto con ese código en esta empresa");

                productoExiste.Nombre = producto.Nombre;
                productoExiste.Codigo = producto.Codigo;
                productoExiste.PrecioCompra = producto.PrecioCompra;
                productoExiste.PrecioVenta = producto.PrecioVenta;
                productoExiste.Stock = producto.Stock;
                productoExiste.StockMinimo = producto.StockMinimo;
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p => p.Id == id && p.EmpresaId == empresaId);

                if (producto == null)
                    return NotFound($"Producto con ID {id} no encontrado");

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
        [HttpGet("bajo-stock")]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductosBajoStock()
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var productos = await _context.Productos
                    .Where(p => p.EmpresaId == empresaId && p.Activo && p.Stock <= p.StockMinimo)
                    .OrderBy(p => p.Stock)
                    .ThenBy(p => p.Nombre)
                    .ToListAsync();

                return Ok(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos con bajo stock");
                return StatusCode(500, "Error interno del servidor");
            }
        }
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetProductosCount()
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var count = await _context.Productos
                    .CountAsync(p => p.EmpresaId == empresaId && p.Activo);

                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conteo de productos");
                return StatusCode(500, "Error interno del servidor");
            }
        }
        [HttpPut("{id}/stock")]
        public async Task<IActionResult> ActualizarStock(int id, [FromBody] int cantidad)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p => p.Id == id && p.EmpresaId == empresaId);

                if (producto == null)
                    return NotFound($"Producto con ID {id} no encontrado");

                producto.Stock = cantidad;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar stock del producto {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }



    }
}
