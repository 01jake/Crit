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
    public class ProveedoresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProveedoresController> _logger;

        public ProveedoresController(ApplicationDbContext context, ILogger<ProveedoresController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Proveedor>>> Get()
        {
            try
            {
                return await _context.Proveedores
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener proveedores");
                return StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Create(Proveedor proveedor)
        {
            try
            {
                // 🔴 VALIDAR RFC DUPLICADO
                if (await _context.Proveedores.AnyAsync(p => p.RFC == proveedor.RFC))
                    return BadRequest("Ya existe un proveedor con ese RFC");

                proveedor.Id = 0;
                proveedor.FechaRegistro = DateTime.Now;

                _context.Proveedores.Add(proveedor);
                await _context.SaveChangesAsync();

                return Ok(proveedor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear proveedor");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Proveedor proveedor)
        {
            try
            {
                if (id != proveedor.Id)
                    return BadRequest();

                var existe = await _context.Proveedores.FindAsync(id);

                if (existe == null)
                    return NotFound();

                // 🔴 VALIDAR RFC DUPLICADO
                if (await _context.Proveedores.AnyAsync(p => p.RFC == proveedor.RFC && p.Id != id))
                    return BadRequest("RFC ya registrado");

                _context.Entry(existe).CurrentValues.SetValues(proveedor);

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar proveedor");
                return StatusCode(500);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var proveedor = await _context.Proveedores.FindAsync(id);

                if (proveedor == null)
                    return NotFound();

                // 🔥 NO BORRAR → DESACTIVAR
                proveedor.Activo = false;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar proveedor");
                return StatusCode(500);
            }
        }
    }

}
