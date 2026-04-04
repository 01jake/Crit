using Crit.Server.Data;
using Crit.Shared.DTOs;
using Crit.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GastosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GastosController> _logger;
        private readonly IEmpresaProvider _empresaProvider;

        public GastosController(
            ApplicationDbContext context,
            ILogger<GastosController> logger,
            IEmpresaProvider empresaProvider)
        {
            _context = context;
            _logger = logger;
            _empresaProvider = empresaProvider;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Gasto>>> GetGastos()
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();
                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var gastos = await _context.Gastos
                    .Include(x => x.Proveedor)
                    .Where(x => x.EmpresaId == empresaId && x.Activo)
                    .OrderByDescending(x => x.Fecha)
                    .ToListAsync();

                return Ok(gastos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener gastos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Gasto>> GetGasto(int id)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();
                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var gasto = await _context.Gastos
                    .Include(x => x.Proveedor)
                    .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);

                if (gasto == null)
                    return NotFound("Gasto no encontrado");

                return Ok(gasto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener gasto {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost]
        public async Task<ActionResult> CrearGasto([FromBody] RegistrarGastoDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();
                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var caja = await _context.CajaSesiones
                    .Where(x => x.EmpresaId == empresaId)
                    .OrderByDescending(x => x.FechaApertura)
                    .FirstOrDefaultAsync(x => x.Estado == "Abierta");

                if (caja == null)
                    return BadRequest("No hay una caja abierta");

                if (dto.Monto <= 0)
                    return BadRequest("El monto debe ser mayor a cero");

                if (dto.ProveedorId.HasValue)
                {
                    var proveedorExiste = await _context.Proveedores
                        .AnyAsync(x => x.Id == dto.ProveedorId.Value && x.EmpresaId == empresaId);

                    if (!proveedorExiste)
                        return BadRequest("El proveedor no existe");
                }

                var saldoAnterior = caja.SaldoCalculado;
                var saldoPosterior = saldoAnterior - dto.Monto;

                var gasto = new Gasto
                {
                    EmpresaId = empresaId,
                    Fecha = dto.Fecha == default ? DateTime.Now : dto.Fecha,
                    Concepto = dto.Concepto,
                    Categoria = dto.Categoria,
                    Monto = dto.Monto,
                    MetodoPago = dto.MetodoPago,
                    Referencia = dto.Referencia,
                    Observaciones = dto.Observaciones,
                    ProveedorId = dto.ProveedorId,
                    CajaSesionId = caja.Id,
                    Activo = true
                };

                _context.Gastos.Add(gasto);
                await _context.SaveChangesAsync();

                var movimiento = new CajaMovimiento
                {
                    EmpresaId = empresaId,
                    CajaSesionId = caja.Id,
                    Fecha = gasto.Fecha,
                    Tipo = "Egreso",
                    Origen = "Gasto",
                    Monto = gasto.Monto,
                    SaldoAnterior = saldoAnterior,
                    SaldoPosterior = saldoPosterior,
                    GastoId = gasto.Id,
                    Referencia = gasto.Referencia,
                    Concepto = gasto.Concepto,
                    MetodoPago = gasto.MetodoPago,
                    Activo = true
                };

                _context.CajaMovimientos.Add(movimiento);
                caja.TotalEgresos += gasto.Monto;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(gasto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear gasto");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("{id}/cancelar")]
        public async Task<ActionResult> CancelarGasto(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();
                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var gasto = await _context.Gastos
                    .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);

                if (gasto == null)
                    return NotFound("Gasto no encontrado");

                if (!gasto.Activo)
                    return BadRequest("El gasto ya está cancelado");

                var caja = await _context.CajaSesiones
                    .FirstOrDefaultAsync(x => x.Id == gasto.CajaSesionId && x.EmpresaId == empresaId);

                var movimiento = await _context.CajaMovimientos
                    .FirstOrDefaultAsync(x => x.GastoId == gasto.Id && x.Activo && x.EmpresaId == empresaId);

                gasto.Activo = false;

                if (movimiento != null)
                {
                    movimiento.Activo = false;

                    if (caja != null)
                        caja.TotalEgresos -= movimiento.Monto;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Gasto cancelado correctamente" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al cancelar gasto {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
