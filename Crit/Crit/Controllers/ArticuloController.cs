using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crit.Data;
using Crit.Server.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ArticulosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ArticulosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Articulo>>> GetArticulos()
        {
            try
            {
                var articulos = await _context.Articulos
                    .Include(a => a.UsuarioQueRegistro)
                    .Select(a => new Articulo
                    {
                        Id = a.Id,
                        Codigo = a.Codigo,
                        Nombre = a.Nombre,
                        Descripcion = a.Descripcion,
                        Ubicacion = a.Ubicacion,
                        Uso = a.Uso,
                        NivelPriorizacion = a.NivelPriorizacion,
                        FechaRegistro = a.FechaRegistro,
                        UsuarioQueRegistroId = a.UsuarioQueRegistroId,
                        UsuarioQueRegistroUserName = a.UsuarioQueRegistro!.UserName
                    })
                    .OrderByDescending(a => a.FechaRegistro)
                    .ToListAsync();

                return Ok(articulos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Articulo>> GetArticulo(int id)
        {
            try
            {
                var articuloEntity = await _context.Articulos
                    .Include(a => a.UsuarioQueRegistro)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (articuloEntity == null)
                {
                    return NotFound($"Artículo con ID {id} no encontrado.");
                }

                var articulo = new Articulo
                {
                    Id = articuloEntity.Id,
                    Codigo = articuloEntity.Codigo,
                    Nombre = articuloEntity.Nombre,
                    Descripcion = articuloEntity.Descripcion,
                    Ubicacion = articuloEntity.Ubicacion,
                    Uso = articuloEntity.Uso,
                    NivelPriorizacion = articuloEntity.NivelPriorizacion,
                    FechaRegistro = articuloEntity.FechaRegistro,
                    UsuarioQueRegistroId = articuloEntity.UsuarioQueRegistroId,
                    UsuarioQueRegistroUserName = articuloEntity.UsuarioQueRegistro?.UserName
                };

                return Ok(articulo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Articulo>> PostArticulo(Articulo articulo)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized("Usuario no autenticado.");
                }

                // Verificar si el código ya existe
                var existeCodigo = await _context.Articulos.AnyAsync(a => a.Codigo == articulo.Codigo);
                if (existeCodigo)
                {
                    return BadRequest($"Ya existe un artículo con el código {articulo.Codigo}");
                }

                var articuloEntity = new ArticuloEntity
                {
                    Codigo = articulo.Codigo,
                    Nombre = articulo.Nombre,
                    Descripcion = articulo.Descripcion,
                    Ubicacion = articulo.Ubicacion,
                    Uso = articulo.Uso,
                    NivelPriorizacion = articulo.NivelPriorizacion,
                    FechaRegistro = DateTime.Now,
                    UsuarioQueRegistroId = user.Id
                };

                _context.Articulos.Add(articuloEntity);
                await _context.SaveChangesAsync();

                var articuloResponse = new Articulo
                {
                    Id = articuloEntity.Id,
                    Codigo = articuloEntity.Codigo,
                    Nombre = articuloEntity.Nombre,
                    Descripcion = articuloEntity.Descripcion,
                    Ubicacion = articuloEntity.Ubicacion,
                    Uso = articuloEntity.Uso,
                    NivelPriorizacion = articuloEntity.NivelPriorizacion,
                    FechaRegistro = articuloEntity.FechaRegistro,
                    UsuarioQueRegistroId = articuloEntity.UsuarioQueRegistroId,
                    UsuarioQueRegistroUserName = user.UserName
                };

                return CreatedAtAction(nameof(GetArticulo), new { id = articuloResponse.Id }, articuloResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutArticulo(int id, Articulo articulo)
        {
            if (id != articulo.Id)
            {
                return BadRequest("El ID no coincide.");
            }

            try
            {
                var existingArticulo = await _context.Articulos.FindAsync(id);
                if (existingArticulo == null)
                {
                    return NotFound();
                }

                // Verificar código único (excepto el actual)
                var existeCodigo = await _context.Articulos.AnyAsync(a => a.Codigo == articulo.Codigo && a.Id != id);
                if (existeCodigo)
                {
                    return BadRequest($"Ya existe otro artículo con el código {articulo.Codigo}");
                }

                existingArticulo.Codigo = articulo.Codigo;
                existingArticulo.Nombre = articulo.Nombre;
                existingArticulo.Descripcion = articulo.Descripcion;
                existingArticulo.Ubicacion = articulo.Ubicacion;
                existingArticulo.Uso = articulo.Uso;
                existingArticulo.NivelPriorizacion = articulo.NivelPriorizacion;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticuloExists(id))
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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteArticulo(int id)
        {
            try
            {
                var articulo = await _context.Articulos.FindAsync(id);
                if (articulo == null)
                {
                    return NotFound();
                }

                _context.Articulos.Remove(articulo);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        private bool ArticuloExists(int id)
        {
            return _context.Articulos.Any(e => e.Id == id);
        }
    }
}