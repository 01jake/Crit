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

    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClientesController> _logger;

        public ClientesController(ApplicationDbContext context, ILogger<ClientesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/clientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            try
            {
                var clientes = await _context.Clientes
                    .OrderBy(c => c.Nombre)
                    .ToListAsync();
                return Ok(clientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/clientes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> GetCliente(int id)
        {
            try
            {
                var cliente = await _context.Clientes.FindAsync(id);

                if (cliente == null)
                {
                    return NotFound($"Cliente con ID {id} no encontrado");
                }

                return Ok(cliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cliente {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/clientes/activos
        [HttpGet("activos")]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientesActivos()
        {
            try
            {
                var clientes = await _context.Clientes
                    .Where(c => c.Activo)
                    .OrderBy(c => c.Nombre)
                    .ToListAsync();
                return Ok(clientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes activos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/clientes
        [HttpPost]
        public async Task<ActionResult<Cliente>> CreateCliente([FromBody] Cliente cliente)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validar email único
                var emailExiste = await _context.Clientes
                    .AnyAsync(c => c.Email == cliente.Email);

                if (emailExiste)
                {
                    return BadRequest("Ya existe un cliente con ese email");
                }

                cliente.FechaRegistro = DateTime.Now;
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, cliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cliente");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/clientes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCliente(int id, [FromBody] Cliente cliente)
        {
            try
            {
                if (id != cliente.Id)
                {
                    return BadRequest("El ID no coincide");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var clienteExiste = await _context.Clientes.FindAsync(id);
                if (clienteExiste == null)
                {
                    return NotFound($"Cliente con ID {id} no encontrado");
                }

                // Validar email único
                var emailExiste = await _context.Clientes
                    .AnyAsync(c => c.Email == cliente.Email && c.Id != id);

                if (emailExiste)
                {
                    return BadRequest("Ya existe otro cliente con ese email");
                }

                // Actualizar propiedades
                clienteExiste.Nombre = cliente.Nombre;
                clienteExiste.Email = cliente.Email;
                clienteExiste.Telefono = cliente.Telefono;
                clienteExiste.RFC = cliente.RFC;
                clienteExiste.Direccion = cliente.Direccion;
                clienteExiste.Activo = cliente.Activo;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cliente {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/clientes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            try
            {
                var cliente = await _context.Clientes.FindAsync(id);
                if (cliente == null)
                {
                    return NotFound($"Cliente con ID {id} no encontrado");
                }

                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cliente {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }


        [HttpGet("count")]
        public async Task<ActionResult<int>> GetClientesCount()
        {
            var count = await _context.Clientes.CountAsync(c => c.Activo);
            return Ok(count);
        }
    }
}
