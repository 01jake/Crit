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
                    .Include(p => p.Proveedor)
                    .Where(p => p.EmpresaId == empresaId)
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                await SincronizarStockDesdeInventarioAsync(productos, empresaId);

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
                    .Include(p => p.Proveedor)
                    .FirstOrDefaultAsync(p => p.Id == id && p.EmpresaId == empresaId);

                if (producto == null)
                    return NotFound($"Producto con ID {id} no encontrado");

                await SincronizarStockDesdeInventarioAsync(producto, empresaId);

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
                    .Include(p => p.Proveedor)
                    .Where(p => p.EmpresaId == empresaId && p.Activo)
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                await SincronizarStockDesdeInventarioAsync(productos, empresaId);

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

                if (producto.ProveedorId.HasValue)
                {
                    var proveedorExiste = await _context.Proveedores
                        .AnyAsync(p => p.Id == producto.ProveedorId.Value && p.EmpresaId == empresaId);

                    if (!proveedorExiste)
                        return BadRequest("El proveedor seleccionado no existe");
                }

                var codigoExiste = await _context.Productos
                    .AnyAsync(p => p.Codigo == producto.Codigo && p.EmpresaId == empresaId);

                if (codigoExiste)
                    return BadRequest("Ya existe un producto con ese código en esta empresa");

                producto.EmpresaId = empresaId;

                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();

                var errorInventario = await SincronizarInventarioDesdeStockManualAsync(producto, empresaId);
                if (!string.IsNullOrWhiteSpace(errorInventario))
                    return BadRequest(errorInventario);

                await _context.SaveChangesAsync();
                await SincronizarStockDesdeInventarioAsync(producto, empresaId);
                await _context.SaveChangesAsync();

                await _context.Entry(producto).Reference(p => p.Proveedor).LoadAsync();

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

                if (producto.ProveedorId.HasValue)
                {
                    var proveedorExiste = await _context.Proveedores
                        .AnyAsync(p => p.Id == producto.ProveedorId.Value && p.EmpresaId == empresaId);

                    if (!proveedorExiste)
                        return BadRequest("El proveedor seleccionado no existe");
                }

                productoExiste.ProveedorId = producto.ProveedorId;
                productoExiste.Nombre = producto.Nombre;
                productoExiste.Codigo = producto.Codigo;
                productoExiste.Descripcion = producto.Descripcion;
                productoExiste.Categoria = producto.Categoria;
                productoExiste.Unidad = producto.Unidad;
                productoExiste.PrecioCompra = producto.PrecioCompra;
                productoExiste.PrecioVenta = producto.PrecioVenta;
                productoExiste.StockMinimo = producto.StockMinimo;
                productoExiste.Activo = producto.Activo;
                productoExiste.Stock = producto.Stock;

                await _context.SaveChangesAsync();

                var errorInventario = await SincronizarInventarioDesdeStockManualAsync(productoExiste, empresaId);
                if (!string.IsNullOrWhiteSpace(errorInventario))
                    return BadRequest(errorInventario);

                await _context.SaveChangesAsync();
                await SincronizarStockDesdeInventarioAsync(productoExiste, empresaId);
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
                    .Include(p => p.Proveedor)
                    .Where(p => p.EmpresaId == empresaId && p.Activo)
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                await SincronizarStockDesdeInventarioAsync(productos, empresaId);

                productos = productos
                    .Where(p => p.Stock <= p.StockMinimo)
                    .OrderBy(p => p.Stock)
                    .ThenBy(p => p.Nombre)
                    .ToList();

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

                var errorInventario = await SincronizarInventarioDesdeStockManualAsync(producto, empresaId);
                if (!string.IsNullOrWhiteSpace(errorInventario))
                    return BadRequest(errorInventario);

                await _context.SaveChangesAsync();
                await SincronizarStockDesdeInventarioAsync(producto, empresaId);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar stock del producto {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private async Task SincronizarStockDesdeInventarioAsync(List<Producto> productos, int empresaId)
        {
            var stockPorProducto = await _context.InventarioPorAlmacen
                .Where(x => x.EmpresaId == empresaId)
                .GroupBy(x => x.ProductoId)
                .Select(g => new
                {
                    ProductoId = g.Key,
                    StockTotal = g.Sum(x => x.Stock)
                })
                .ToDictionaryAsync(x => x.ProductoId, x => x.StockTotal);

            foreach (var producto in productos)
            {
                producto.Stock = stockPorProducto.TryGetValue(producto.Id, out var stockTotal)
                    ? (int)stockTotal
                    : 0;
            }
        }

        private async Task SincronizarStockDesdeInventarioAsync(Producto producto, int empresaId)
        {
            var stockTotal = await _context.InventarioPorAlmacen
                .Where(x => x.EmpresaId == empresaId && x.ProductoId == producto.Id)
                .SumAsync(x => (decimal?)x.Stock) ?? 0m;

            producto.Stock = (int)stockTotal;
        }

        private async Task<Almacen?> ObtenerAlmacenPredeterminadoAsync(int empresaId)
        {
            var almacenPrincipal = await _context.Almacenes
                .Where(a => a.EmpresaId == empresaId && a.Activo && a.Nombre == "PRINCIPAL")
                .FirstOrDefaultAsync();

            if (almacenPrincipal is not null)
                return almacenPrincipal;

            return await _context.Almacenes
                .Where(a => a.EmpresaId == empresaId && a.Activo)
                .OrderBy(a => a.Nombre)
                .FirstOrDefaultAsync();
        }

        private async Task<string?> SincronizarInventarioDesdeStockManualAsync(Producto producto, int empresaId)
        {
            var almacen = await ObtenerAlmacenPredeterminadoAsync(empresaId);

            if (almacen is null)
                return "No existe un almacén activo para registrar el stock del producto.";

            var inventarios = await _context.InventarioPorAlmacen
                .Where(x => x.EmpresaId == empresaId && x.ProductoId == producto.Id)
                .OrderBy(x => x.AlmacenId == almacen.Id ? 0 : 1)
                .ThenBy(x => x.Id)
                .ToListAsync();

            var stockDeseado = producto.Stock < 0 ? 0 : producto.Stock;
            var stockActualTotal = inventarios.Sum(x => x.Stock);

            if (!inventarios.Any())
            {
                _context.InventarioPorAlmacen.Add(new InventarioPorAlmacen
                {
                    EmpresaId = empresaId,
                    ProductoId = producto.Id,
                    AlmacenId = almacen.Id,
                    Stock = stockDeseado,
                    StockMinimo = producto.StockMinimo,
                    StockMaximo = Math.Max(stockDeseado, producto.StockMinimo)
                });

                return null;
            }

            var diferencia = stockDeseado - stockActualTotal;
            var inventarioBase = inventarios.First();

            inventarioBase.Stock += diferencia;
            inventarioBase.StockMinimo = producto.StockMinimo;

            if (inventarioBase.Stock < 0)
                inventarioBase.Stock = 0;

            if (inventarioBase.StockMaximo < inventarioBase.Stock)
                inventarioBase.StockMaximo = inventarioBase.Stock;

            return null;
        }
    }
}
