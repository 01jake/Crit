using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crit.Data;
using Crit.Server.Data;
using Crit.Server.Services;
using Crit.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuejasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<QuejasController> _logger;
        private readonly INotificationService _notificationService;

        public QuejasController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService, IConfiguration configuration, ILogger<QuejasController> logger, INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
            _notificationService = notificationService;

        }

        // GET: api/Quejas (Solo para administradores)
        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<IEnumerable<Queja>>> GetQuejas()
        {
            try
            {
                var quejas = await _context.Quejas
                    .Include(q => q.Cliente)
                    .Select(q => new Queja
                    {
                        Id = q.Id,
                        NombreCliente = q.NombreCliente,
                        NumeroAfiliacion = q.NumeroAfiliacion,
                        Correo = q.Correo,
                        Titulo = q.Titulo,
                        DescripcionQueja = q.DescripcionQueja,
                        Categoria = q.Categoria,
                        Fecha = q.Fecha,
                        Estatus = (EstatusQueja)q.Estatus,
                        Prioridad = (PrioridadQueja)q.Prioridad,
                        ClienteId = q.ClienteId,
                        ClienteUserName = q.Cliente!.UserName
                    })
                    .OrderByDescending(q => q.Fecha)
                    .ToListAsync();

                return Ok(quejas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        // GET: api/Quejas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Queja>> GetQueja(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                var quejaEntity = await _context.Quejas
                    .Include(q => q.Cliente)
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (quejaEntity == null)
                {
                    return NotFound($"Queja con ID {id} no encontrada.");
                }

                var isAdmin = await _userManager.IsInRoleAsync(user, "Administrador");
                if (!isAdmin && quejaEntity.ClienteId != user.Id)
                {
                    return Forbid("No tienes permisos para ver esta queja.");
                }

                var queja = new Queja
                {
                    Id = quejaEntity.Id,
                    NombreCliente = quejaEntity.NombreCliente,
                    NumeroAfiliacion = quejaEntity.NumeroAfiliacion,
                    Correo = quejaEntity.Correo,
                    Titulo = quejaEntity.Titulo,
                    DescripcionQueja = quejaEntity.DescripcionQueja,
                    Categoria = quejaEntity.Categoria,
                    Fecha = quejaEntity.Fecha,
                    Estatus = (EstatusQueja)quejaEntity.Estatus,
                    Prioridad = (PrioridadQueja)quejaEntity.Prioridad,
                    ClienteId = quejaEntity.ClienteId,
                    ClienteUserName = quejaEntity.Cliente?.UserName
                };

                return Ok(queja);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        // GET: api/Quejas/mis-quejas
        [HttpGet("mis-quejas")]
        public async Task<ActionResult<IEnumerable<Queja>>> GetMisQuejas()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                var quejas = await _context.Quejas
                    .Include(q => q.Cliente)
                    .Where(q => q.ClienteId == user.Id)
                    .Select(q => new Queja
                    {
                        Id = q.Id,
                        NombreCliente = q.NombreCliente,
                        NumeroAfiliacion = q.NumeroAfiliacion,
                        Correo = q.Correo,
                        Titulo = q.Titulo,
                        DescripcionQueja = q.DescripcionQueja,
                        Categoria = q.Categoria,
                        Fecha = q.Fecha,
                        Estatus = (EstatusQueja)q.Estatus,
                        Prioridad = (PrioridadQueja)q.Prioridad,
                        ClienteId = q.ClienteId,
                        ClienteUserName = q.Cliente!.UserName
                    })
                    .OrderByDescending(q => q.Fecha)
                    .ToListAsync();

                return Ok(quejas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
        // Post: api/Quejas (Solo para quejas para clientes)
        [HttpPost("publica")]
        [AllowAnonymous] // Permite acceso sin autenticación
        public async Task<IActionResult> PostQuejaPublica([FromBody] QuejaPublica queja)
        {
            try
            {
                var quejaEntity = new QuejaEntity
                {
                    NombreCliente = queja.NombreCliente,
                    Correo = queja.Correo,
                    NumeroAfiliacion = queja.NumeroAfiliacion ?? string.Empty,
                    Titulo = queja.Titulo,
                    DescripcionQueja = queja.DescripcionQueja,
                    Categoria = queja.Categoria,
                    Fecha = DateTime.Now,
                    Estatus = Server.Data.EstatusQueja.Pendiente,
                    Prioridad = Server.Data.PrioridadQueja.Media,
                    ClienteId = null
                };

                _context.Quejas.Add(quejaEntity);
                await _context.SaveChangesAsync();

                // Enviar email para queja anónima
                await EnviarNotificacionQueja(queja.NombreCliente, queja.Correo, queja.Titulo, queja.DescripcionQueja, queja.Categoria, "ANÓNIMA");

                return Ok(new { message = "Queja enviada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
        // POST: api/Quejas
        [HttpPost]
        public async Task<ActionResult<Queja>> PostQueja(Queja queja)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized("Usuario no autenticado.");
                }

                var quejaEntity = new QuejaEntity
                {
                    NombreCliente = queja.NombreCliente,
                    NumeroAfiliacion = queja.NumeroAfiliacion ?? string.Empty,
                    Correo = queja.Correo,
                    Titulo = queja.Titulo,
                    DescripcionQueja = queja.DescripcionQueja,
                    Categoria = queja.Categoria,
                    Fecha = DateTime.Now,
                    Estatus = (Server.Data.EstatusQueja)EstatusQueja.Pendiente,
                    Prioridad = (Server.Data.PrioridadQueja)queja.Prioridad,
                    ClienteId = user.Id
                };

                _context.Quejas.Add(quejaEntity);
                await _context.SaveChangesAsync();

                // Enviar email para queja de usuario registrado
                await EnviarNotificacionQueja(queja.NombreCliente, queja.Correo, queja.Titulo, queja.DescripcionQueja, queja.Categoria, "USUARIO REGISTRADO", user.UserName);

                var quejaResponse = new Queja
                {
                    Id = quejaEntity.Id,
                    NombreCliente = quejaEntity.NombreCliente,
                    NumeroAfiliacion = quejaEntity.NumeroAfiliacion,
                    Correo = quejaEntity.Correo,
                    Titulo = quejaEntity.Titulo,
                    DescripcionQueja = quejaEntity.DescripcionQueja,
                    Categoria = quejaEntity.Categoria,
                    Fecha = quejaEntity.Fecha,
                    Estatus = (EstatusQueja)quejaEntity.Estatus,
                    Prioridad = (PrioridadQueja)quejaEntity.Prioridad,
                    ClienteId = quejaEntity.ClienteId,
                    ClienteUserName = user.UserName
                };

                return CreatedAtAction(nameof(GetQueja), new { id = quejaResponse.Id }, quejaResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

      
        private async Task EnviarNotificacionQueja(string nombreCliente, string correoCliente, string titulo, string descripcion, string categoria, string tipoQueja, string? usuarioRegistrado = null)
        {
            try
            {
                var adminEmail = _configuration["Email:AdminEmail"];
                if (string.IsNullOrWhiteSpace(adminEmail))
                {
                    _logger.LogWarning("No se ha configurado el correo del administrador (Email:AdminEmail). No se enviará el correo de notificación.");
                    return;
                }

                var subject = $"Nueva Queja {tipoQueja} - {titulo}";

                var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #dc3545; border-bottom: 2px solid #dc3545; padding-bottom: 10px;'>
                        Nueva Queja Recibida ({tipoQueja})
                    </h2>
                    
                    <div style='background: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <h3 style='color: #495057; margin-top: 0;'>Información del Cliente</h3>
                        <p><strong>Nombre:</strong> {nombreCliente}</p>
                        <p><strong>Email:</strong> {correoCliente}</p>
                        {(usuarioRegistrado != null ? $"<p><strong>Usuario en Sistema:</strong> {usuarioRegistrado}</p>" : "")}
                    </div>

                    <div style='background: #ffffff; padding: 15px; border: 1px solid #dee2e6; border-radius: 5px; margin: 20px 0;'>
                        <h3 style='color: #495057; margin-top: 0;'>Detalles de la Queja</h3>
                        <p><strong>Categoría:</strong> <span style='background: #007bff; color: white; padding: 2px 8px; border-radius: 3px;'>{categoria}</span></p>
                        <p><strong>Título:</strong> {titulo}</p>
                        <p><strong>Descripción:</strong></p>
                        <div style='background: #f8f9fa; padding: 10px; border-left: 4px solid #007bff; margin: 10px 0;'>
                            {descripcion.Replace("\n", "<br>")}
                        </div>
                    </div>

                    <div style='background: #e9ecef; padding: 10px; border-radius: 5px; text-align: center;'>
                        <small style='color: #6c757d;'>
                            Queja recibida el {DateTime.Now:dd/MM/yyyy} a las {DateTime.Now:HH:mm}
                        </small>
                    </div>
                </div>
                ";

                await _emailService.SendEmailAsync(adminEmail, subject, body);
                await _notificationService.NotifyNewComplaint(nombreCliente, titulo, tipoQueja);
            }
            catch (Exception ex)
            {
                // Log el error pero no fallar la creación de la queja
                _logger.LogError(ex, "Error enviando notificación de email para queja");
                _logger.LogError(ex, "Error enviando notificación para queja");
            }
        }

        // PUT: api/Quejas/5/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> UpdateQuejaStatus(int id, [FromBody] EstatusQueja nuevoEstatus)
        {
            try
            {
                var queja = await _context.Quejas.FindAsync(id);
                if (queja == null)
                {
                    return NotFound($"Queja con ID {id} no encontrada.");
                }

                queja.Estatus = (Server.Data.EstatusQueja)nuevoEstatus;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        // DELETE: api/Quejas/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteQueja(int id)
        {
            try
            {
                var queja = await _context.Quejas.FindAsync(id);
                if (queja == null)
                {
                    return NotFound($"Queja con ID {id} no encontrada.");
                }

                _context.Quejas.Remove(queja);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

    }

}