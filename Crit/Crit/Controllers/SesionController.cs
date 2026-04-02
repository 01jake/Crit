using Crit.Data;
using Crit.Server.Data;
using Crit.Shared.DTOs;
using Crit.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SesionController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmpresaProvider _empresaProvider;
        private readonly ApplicationDbContext _context;

        public SesionController(
            UserManager<ApplicationUser> userManager,
            IEmpresaProvider empresaProvider,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _empresaProvider = empresaProvider;
            _context = context;
        }

        [HttpGet("empresa-actual")]
        public async Task<ActionResult<EmpresaSesionDto>> GetEmpresaActual()
        {
            var userId = _empresaProvider.GetUserId();
            var empresaId = await _empresaProvider.GetEmpresaIdAsync();

            if (string.IsNullOrWhiteSpace(userId) || empresaId <= 0)
                return Unauthorized("No se pudo determinar la empresa del usuario.");

            var user = await _userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId);

            var empresa = await _context.Empresas
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == empresaId);

            if (user == null || empresa == null)
                return NotFound("No se encontró la sesión de empresa.");

            return Ok(new EmpresaSesionDto
            {
                EmpresaId = empresa.Id,
                EmpresaNombre = empresa.Nombre,
                Email = user.Email
            });
        }
    }
}
