using Crit.Data;
using Crit.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsuariosController : ControllerBase
    {
        private static readonly string[] RolesPermitidos = ["Admin", "Supervisor", "Usuario"];

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmpresaProvider _empresaProvider;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(
            UserManager<ApplicationUser> userManager,
            IEmpresaProvider empresaProvider,
            ILogger<UsuariosController> logger)
        {
            _userManager = userManager;
            _empresaProvider = empresaProvider;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioEmpresaDto>>> GetUsuarios()
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var usuarios = await _userManager.Users
                    .Where(x => x.EmpresaId == empresaId)
                    .OrderBy(x => x.NombreCompleto)
                    .ThenBy(x => x.Email)
                    .ToListAsync();

                var resultado = new List<UsuarioEmpresaDto>();

                foreach (var user in usuarios)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    resultado.Add(new UsuarioEmpresaDto
                    {
                        Id = user.Id,
                        NombreCompleto = user.NombreCompleto ?? string.Empty,
                        Email = user.Email ?? string.Empty,
                        EmpresaId = user.EmpresaId,
                        Rol = roles.FirstOrDefault() ?? string.Empty,
                        Activo = !user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow
                    });
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios de la empresa");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioEmpresaDto>> GetUsuario(string id)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var user = await _userManager.Users
                    .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);

                if (user == null)
                    return NotFound("Usuario no encontrado.");

                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new UsuarioEmpresaDto
                {
                    Id = user.Id,
                    NombreCompleto = user.NombreCompleto ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    EmpresaId = user.EmpresaId,
                    Rol = roles.FirstOrDefault() ?? string.Empty,
                    Activo = !user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearUsuario([FromBody] Shared.Models.CrearUsuarioDto dto)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!RolesPermitidos.Contains(dto.Rol))
                    return BadRequest("Rol no válido.");

                var emailExiste = await _userManager.Users
                    .AnyAsync(x => x.Email == dto.Email);

                if (emailExiste)
                    return BadRequest("Ya existe un usuario con ese correo.");

                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    NombreCompleto = dto.NombreCompleto,
                    EmpresaId = empresaId,
                    EmailConfirmed = true,
                    LockoutEnabled = true
                };

                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                var roleResult = await _userManager.AddToRoleAsync(user, dto.Rol);

                if (!roleResult.Succeeded)
                    return BadRequest(roleResult.Errors);

                return Ok(new
                {
                    message = "Usuario creado correctamente",
                    userId = user.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("{id}/rol")]
        public async Task<IActionResult> CambiarRol(string id, [FromBody] CambiarRolUsuarioDto dto)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                if (!RolesPermitidos.Contains(dto.Rol))
                    return BadRequest("Rol no válido.");

                var user = await _userManager.Users
                    .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);

                if (user == null)
                    return NotFound("Usuario no encontrado.");

                var rolesActuales = await _userManager.GetRolesAsync(user);

                if (rolesActuales.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesActuales);
                    if (!removeResult.Succeeded)
                        return BadRequest(removeResult.Errors);
                }

                var addResult = await _userManager.AddToRoleAsync(user, dto.Rol);
                if (!addResult.Succeeded)
                    return BadRequest(addResult.Errors);

                return Ok(new { message = "Rol actualizado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar rol del usuario {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("{id}/desactivar")]
        public async Task<IActionResult> DesactivarUsuario(string id)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var user = await _userManager.Users
                    .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);

                if (user == null)
                    return NotFound("Usuario no encontrado.");

                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                return Ok(new { message = "Usuario desactivado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar usuario {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("{id}/activar")]
        public async Task<IActionResult> ActivarUsuario(string id)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var user = await _userManager.Users
                    .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);

                if (user == null)
                    return NotFound("Usuario no encontrado.");

                user.LockoutEnd = null;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                return Ok(new { message = "Usuario activado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al activar usuario {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
