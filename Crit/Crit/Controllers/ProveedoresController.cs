using Crit.Server.Data;
using Crit.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProveedoresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProveedoresController> _logger;
        private readonly IEmpresaProvider _empresaProvider;

        public ProveedoresController(
            ApplicationDbContext context,
            ILogger<ProveedoresController> logger,
            IEmpresaProvider empresaProvider)
        {
            _context = context;
            _logger = logger;
            _empresaProvider = empresaProvider;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Proveedor>>> GetProveedores()
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var proveedores = await _context.Proveedores
                    .Where(p => p.EmpresaId == empresaId)
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                return Ok(proveedores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener proveedores");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Proveedor>> GetProveedor(int id)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var proveedor = await _context.Proveedores
                    .FirstOrDefaultAsync(p => p.Id == id && p.EmpresaId == empresaId);

                if (proveedor == null)
                    return NotFound($"Proveedor con ID {id} no encontrado");

                return Ok(proveedor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener proveedor {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("activos")]
        public async Task<ActionResult<IEnumerable<Proveedor>>> GetProveedoresActivos()
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var proveedores = await _context.Proveedores
                    .Where(p => p.EmpresaId == empresaId && p.Activo)
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                return Ok(proveedores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener proveedores activos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Proveedor>> CreateProveedor([FromBody] Proveedor proveedor)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var emailExiste = await _context.Proveedores
                    .AnyAsync(p => p.Email == proveedor.Email && p.EmpresaId == empresaId);

                if (emailExiste)
                    return BadRequest("Ya existe un proveedor con ese email en esta empresa");

                proveedor.EmpresaId = empresaId;

                _context.Proveedores.Add(proveedor);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProveedor), new { id = proveedor.Id }, proveedor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear proveedor");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProveedor(int id, [FromBody] Proveedor proveedor)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                if (id != proveedor.Id)
                    return BadRequest("El ID no coincide");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var proveedorExiste = await _context.Proveedores
                    .FirstOrDefaultAsync(p => p.Id == id && p.EmpresaId == empresaId);

                if (proveedorExiste == null)
                    return NotFound($"Proveedor con ID {id} no encontrado");

                var emailExiste = await _context.Proveedores
                    .AnyAsync(p => p.Email == proveedor.Email && p.Id != id && p.EmpresaId == empresaId);

                if (emailExiste)
                    return BadRequest("Ya existe otro proveedor con ese email en esta empresa");

                proveedorExiste.Nombre = proveedor.Nombre;
                proveedorExiste.Email = proveedor.Email;
                proveedorExiste.Telefono = proveedor.Telefono;
                proveedorExiste.Direccion = proveedor.Direccion;
                proveedorExiste.RFC = proveedor.RFC;
                proveedorExiste.Contacto = proveedor.Contacto;
                proveedorExiste.RegimenFiscal = proveedor.RegimenFiscal;
                proveedorExiste.Activo = proveedor.Activo;
                proveedorExiste.CodigoPostal = proveedor.CodigoPostal;


                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar proveedor {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProveedor(int id)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var proveedor = await _context.Proveedores
                    .FirstOrDefaultAsync(p => p.Id == id && p.EmpresaId == empresaId);

                if (proveedor == null)
                    return NotFound($"Proveedor con ID {id} no encontrado");

                _context.Proveedores.Remove(proveedor);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar proveedor {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
