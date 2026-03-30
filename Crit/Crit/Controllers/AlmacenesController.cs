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
    public class AlmacenesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AlmacenesController> _logger;

        public AlmacenesController(ApplicationDbContext context, ILogger<AlmacenesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Almacen>>> GetAlmacenes()
        {
            try
            {
                var almacenes = await _context.Almacenes
                    .OrderBy(a => a.Nombre)
                    .ToListAsync();

                return Ok(almacenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener almacenes");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Almacen>> GetAlmacen(int id)
        {
            try
            {
                var almacen = await _context.Almacenes.FindAsync(id);

                if (almacen == null)
                    return NotFound($"Almacén con ID {id} no encontrado");

                return Ok(almacen);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener almacén {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("activos")]
        public async Task<ActionResult<IEnumerable<Almacen>>> GetAlmacenesActivos()
        {
            try
            {
                var almacenes = await _context.Almacenes
                    .Where(a => a.Activo)
                    .OrderBy(a => a.Nombre)
                    .ToListAsync();

                return Ok(almacenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener almacenes activos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Almacen>> CreateAlmacen([FromBody] Almacen almacen)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existeNombre = await _context.Almacenes
                    .AnyAsync(a => a.Nombre == almacen.Nombre);

                if (existeNombre)
                    return BadRequest("Ya existe un almacén con ese nombre");

                _context.Almacenes.Add(almacen);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAlmacen), new { id = almacen.Id }, almacen);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear almacén");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAlmacen(int id, [FromBody] Almacen almacen)
        {
            try
            {
                if (id != almacen.Id)
                    return BadRequest("El ID no coincide");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var almacenExiste = await _context.Almacenes.FindAsync(id);
                if (almacenExiste == null)
                    return NotFound($"Almacén con ID {id} no encontrado");

                var existeNombre = await _context.Almacenes
                    .AnyAsync(a => a.Nombre == almacen.Nombre && a.Id != id);

                if (existeNombre)
                    return BadRequest("Ya existe otro almacén con ese nombre");

                almacenExiste.Nombre = almacen.Nombre;
                almacenExiste.Clave = almacen.Clave;
                almacenExiste.Direccion = almacen.Direccion;
                almacenExiste.Activo = almacen.Activo;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar almacén {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlmacen(int id)
        {
            try
            {
                var almacen = await _context.Almacenes.FindAsync(id);
                if (almacen == null)
                    return NotFound($"Almacén con ID {id} no encontrado");

                var tieneInventario = await _context.InventarioPorAlmacen
                    .AnyAsync(x => x.AlmacenId == id && x.Stock > 0);

                if (tieneInventario)
                    return BadRequest("No se puede eliminar el almacén porque tiene inventario asignado");

                _context.Almacenes.Remove(almacen);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar almacén {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}