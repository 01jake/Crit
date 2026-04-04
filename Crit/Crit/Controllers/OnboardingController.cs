using Crit.Data;
using Crit.Server.Data;
using Crit.Shared.DTOs;
using Crit.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OnboardingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<OnboardingController> _logger;

        public OnboardingController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<OnboardingController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost("register-empresa")]
        public async Task<IActionResult> RegisterEmpresa([FromBody] RegisterEmpresaDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var emailExiste = await _userManager.Users
                    .AnyAsync(x => x.Email == dto.Email);

                if (emailExiste)
                    return BadRequest("Ya existe un usuario con ese correo.");

                var empresa = new Empresa
                {
                    Nombre = dto.EmpresaNombre,
                    RFC = dto.RFC,
                    Activa = true,
                    FechaRegistro = DateTime.Now
                };

                _context.Empresas.Add(empresa);
                await _context.SaveChangesAsync();

                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    NombreCompleto = dto.NombreCompleto,
                    EmpresaId = empresa.Id,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(result.Errors);
                }

                await _userManager.AddToRoleAsync(user, "Admin");

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Empresa y usuario administrador creados correctamente",
                    empresaId = empresa.Id,
                    userId = user.Id
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al registrar empresa");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
