using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crit.Data;
using Crit.Server.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Crit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Productos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {
            try
            {
                var productos = await _context.Productos
                    .Include(p => p.UsuarioQueRegistro)
                    .Select(p => new Producto // Mapear de Entity a DTO
                    {
                        Id = p.Id,
                        Nombre = p.Nombre,
                        Cantidad = p.Cantidad,
                        Categoria = p.Categoria,
                        FechaIngreso = p.FechaIngreso,
                        UsuarioQueRegistroId = p.UsuarioQueRegistroId,
                        UsuarioQueRegistroUserName = p.UsuarioQueRegistro!.UserName
                    })
                    .ToListAsync();

                return Ok(productos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        // GET: api/Productos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Producto>> GetProducto(int id)
        {
            try
            {
                var productoEntity = await _context.Productos
                    .Include(p => p.UsuarioQueRegistro)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (productoEntity == null)
                {
                    return NotFound($"Producto con ID {id} no encontrado.");
                }

                var producto = new Producto
                {
                    Id = productoEntity.Id,
                    Nombre = productoEntity.Nombre,
                    Cantidad = productoEntity.Cantidad,
                    Categoria = productoEntity.Categoria,
                    FechaIngreso = productoEntity.FechaIngreso,
                    UsuarioQueRegistroId = productoEntity.UsuarioQueRegistroId,
                    UsuarioQueRegistroUserName = productoEntity.UsuarioQueRegistro?.UserName
                };

                return Ok(producto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        // PUT: api/Productos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto(int id, Producto producto)
        {
            if (id != producto.Id)
            {
                return BadRequest("El ID no coincide.");
            }

            try
            {
                var existingProducto = await _context.Productos.FindAsync(id);
                if (existingProducto == null)
                {
                    return NotFound();
                }

                // Actualizar solo campos permitidos
                existingProducto.Nombre = producto.Nombre;
                existingProducto.Cantidad = producto.Cantidad;
                existingProducto.Categoria = producto.Categoria;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductoExists(id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        // POST: api/Productos
        [HttpPost]
        public async Task<ActionResult<Producto>> PostProducto(Producto producto)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized("Usuario no autenticado.");
                }

                var productoEntity = new ProductoEntity
                {
                    Nombre = producto.Nombre,
                    Cantidad = producto.Cantidad,
                    Categoria = producto.Categoria,
                    FechaIngreso = DateTime.Now,
                    UsuarioQueRegistroId = user.Id
                };

                _context.Productos.Add(productoEntity);
                await _context.SaveChangesAsync();

                // Retornar DTO
                var productoResponse = new Producto
                {
                    Id = productoEntity.Id,
                    Nombre = productoEntity.Nombre,
                    Cantidad = productoEntity.Cantidad,
                    Categoria = productoEntity.Categoria,
                    FechaIngreso = productoEntity.FechaIngreso,
                    UsuarioQueRegistroId = productoEntity.UsuarioQueRegistroId,
                    UsuarioQueRegistroUserName = user.UserName
                };

                return CreatedAtAction(nameof(GetProducto), new { id = productoResponse.Id }, productoResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        // DELETE: api/Productos/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                {
                    return NotFound();
                }

                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }
    }
}
