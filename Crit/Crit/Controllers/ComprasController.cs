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
    public class ComprasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ComprasController> _logger;

        public ComprasController(ApplicationDbContext context, ILogger<ComprasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/compras
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Compra>>> GetCompras()
        {
            try
            {
                var compras = await _context.Compra
                    .Include(c => c.Proveedor)
                    .Include(c => c.Detalles)
                        .ThenInclude(d => d.Producto)
                    .OrderByDescending(c => c.Fecha)
                    .ToListAsync();

                return Ok(compras);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener compras");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/compras
        [HttpPost]
        public async Task<IActionResult> CrearCompra(Compra compra)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                compra.Fecha = DateTime.Now;

                decimal total = 0;

                foreach (var d in compra.Detalles)
                {
                    var producto = await _context.Productos.FindAsync(d.ProductoId);

                    if (producto == null)
                        return BadRequest("Producto no existe");

                    int stockAnterior = producto.Stock;

                    // 🔥 REGLA #1: recalcular SIEMPRE
                    d.Subtotal = d.Cantidad * d.PrecioUnitario;

                    // 🔥 INVENTARIO
                    producto.Stock += d.Cantidad;

                    // 🔥 COSTO PROMEDIO
                    producto.PrecioCompra =
                        ((producto.PrecioCompra * stockAnterior) +
                        (d.PrecioUnitario * d.Cantidad))
                        / (stockAnterior + d.Cantidad);

                    // 🔥 KARDEX
                    //_context.Kardex.Add(new Kardex
                    //{
                    //    ProductoId = producto.Id,
                    //    Fecha = DateTime.Now,
                    //    TipoMovimiento = "COMPRA",
                    //    Cantidad = d.Cantidad,
                    //    CostoUnitario = d.PrecioUnitario,
                    //    StockAnterior = stockAnterior,
                    //    StockNuevo = producto.Stock
                    //});

                    total += d.Subtotal;
                }
                compra.Subtotal = total;
                compra.IVA = total * 0.16m;
                compra.Total = compra.Subtotal + compra.IVA;

                _context.Compra.Add(compra);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(compra);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("historial")]
        public async Task<IActionResult> Historial()
        {
            var data = await _context.Compra
                .Include(c => c.Proveedor)
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();

            return Ok(data);
        }
        // DELETE: api/compras/5 (rollback stock)
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelarCompra(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var compra = await _context.Compra
                    .Include(c => c.Detalles)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (compra == null)
                    return NotFound();

                foreach (var d in compra.Detalles)
                {
                    var producto = await _context.Productos.FindAsync(d.ProductoId);

                    if (producto != null)
                    {
                        // 🔥 REVERSAR STOCK
                        producto.Stock -= d.Cantidad;
                    }
                }

                _context.Compra.Remove(compra);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(ex, "Error al cancelar compra");

                return StatusCode(500, "Error al cancelar compra");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompra(int id)
        {
            var compra = await _context.Compra
                .Include(c => c.Proveedor)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (compra == null)
                return NotFound();

            return Ok(compra);
        }

    }
}
