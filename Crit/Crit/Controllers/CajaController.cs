using Crit.Server.Data;
using Crit.Shared.DTOs;
using Crit.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CajaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CajaController> _logger;
        private readonly IEmpresaProvider _empresaProvider;

        public CajaController(
            ApplicationDbContext context,
            ILogger<CajaController> logger,
            IEmpresaProvider empresaProvider)
        {
            _context = context;
            _logger = logger;
            _empresaProvider = empresaProvider;
        }

        [HttpGet("actual")]
        public async Task<ActionResult<CajaSesion>> GetCajaActual()
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var caja = await _context.CajaSesiones
                    .Include(x => x.Movimientos.Where(m => m.Activo))
                    .Where(x => x.EmpresaId == empresaId)
                    .OrderByDescending(x => x.FechaApertura)
                    .FirstOrDefaultAsync(x => x.Estado == "Abierta");

                if (caja == null)
                    return NotFound("No hay una caja abierta");

                return Ok(caja);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la caja actual");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("resumen")]
        public async Task<ActionResult<CajaResumenDto>> GetResumenCaja()
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var hoy = DateTime.Today;

                var caja = await _context.CajaSesiones
                    .AsNoTracking()
                    .Where(x => x.EmpresaId == empresaId)
                    .OrderByDescending(x => x.FechaApertura)
                    .FirstOrDefaultAsync(x => x.Estado == "Abierta");

                var movimientosHoy = await _context.CajaMovimientos
                    .AsNoTracking()
                    .Where(x => x.EmpresaId == empresaId && x.Activo && x.Fecha.Date == hoy)
                    .ToListAsync();

                var resumen = new CajaResumenDto
                {
                    CajaAbierta = caja != null,
                    MontoInicial = caja?.MontoInicial ?? 0m,
                    IngresosHoy = movimientosHoy.Where(x => x.Tipo == "Ingreso").Sum(x => x.Monto),
                    EgresosHoy = movimientosHoy.Where(x => x.Tipo == "Egreso").Sum(x => x.Monto),
                    SaldoActual = caja?.SaldoCalculado ?? 0m
                };

                return Ok(resumen);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener resumen de caja");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("movimientos")]
        public async Task<ActionResult<IEnumerable<CajaMovimiento>>> GetMovimientos()
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var movimientos = await _context.CajaMovimientos
                    .Where(x => x.EmpresaId == empresaId && x.Activo)
                    .OrderByDescending(x => x.Fecha)
                    .ToListAsync();

                return Ok(movimientos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener movimientos de caja");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("abrir")]
        public async Task<ActionResult> AbrirCaja([FromBody] AperturaCajaDto dto)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var cajaAbierta = await _context.CajaSesiones
                    .AnyAsync(x => x.EmpresaId == empresaId && x.Estado == "Abierta");

                if (cajaAbierta)
                    return BadRequest("Ya existe una caja abierta");

                var caja = new CajaSesion
                {
                    EmpresaId = empresaId,
                    FechaApertura = DateTime.Now,
                    MontoInicial = dto.MontoInicial,
                    MontoFinal = 0m,
                    TotalIngresos = 0m,
                    TotalEgresos = 0m,
                    Estado = "Abierta",
                    Observaciones = dto.Observaciones
                };

                _context.CajaSesiones.Add(caja);
                await _context.SaveChangesAsync();

                if (dto.MontoInicial > 0)
                {
                    var movimiento = new CajaMovimiento
                    {
                        EmpresaId = empresaId,
                        CajaSesionId = caja.Id,
                        Fecha = DateTime.Now,
                        Tipo = "Ingreso",
                        Origen = "Apertura",
                        Monto = dto.MontoInicial,
                        SaldoAnterior = 0m,
                        SaldoPosterior = dto.MontoInicial,
                        Concepto = "Apertura de caja",
                        Activo = true
                    };

                    _context.CajaMovimientos.Add(movimiento);
                    caja.TotalIngresos = dto.MontoInicial;

                    await _context.SaveChangesAsync();
                }

                return Ok(caja);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al abrir caja");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("cerrar")]
        public async Task<ActionResult> CerrarCaja([FromBody] CierreCajaDto dto)
        {
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
                    return NotFound("No hay una caja abierta");

                caja.FechaCierre = DateTime.Now;
                caja.MontoFinal = dto.MontoFinal;
                caja.Estado = "Cerrada";
                caja.Observaciones = string.IsNullOrWhiteSpace(dto.Observaciones)
                    ? caja.Observaciones
                    : dto.Observaciones;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    caja.Id,
                    caja.FechaApertura,
                    caja.FechaCierre,
                    caja.MontoInicial,
                    caja.MontoFinal,
                    caja.TotalIngresos,
                    caja.TotalEgresos,
                    caja.SaldoCalculado,
                    Diferencia = dto.MontoFinal - caja.SaldoCalculado
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar caja");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("movimiento-manual")]
        public async Task<ActionResult> RegistrarMovimientoManual([FromBody] CajaMovimiento movimiento)
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
                    return BadRequest("No hay caja abierta");

                if (movimiento.Monto <= 0)
                    return BadRequest("El monto debe ser mayor a cero");

                var saldoAnterior = caja.SaldoCalculado;
                var saldoPosterior = movimiento.Tipo == "Egreso"
                    ? saldoAnterior - movimiento.Monto
                    : saldoAnterior + movimiento.Monto;

                movimiento.Id = 0;
                movimiento.EmpresaId = empresaId;
                movimiento.CajaSesionId = caja.Id;
                movimiento.Fecha = DateTime.Now;
                movimiento.SaldoAnterior = saldoAnterior;
                movimiento.SaldoPosterior = saldoPosterior;
                movimiento.Activo = true;

                _context.CajaMovimientos.Add(movimiento);

                if (movimiento.Tipo == "Egreso")
                    caja.TotalEgresos += movimiento.Monto;
                else
                    caja.TotalIngresos += movimiento.Monto;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(movimiento);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al registrar movimiento manual");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("cash-flow-real")]
        public async Task<ActionResult<List<FlujoCajaRealDto>>> GetCashFlowReal([FromQuery] int meses = 6)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var fechaInicio = DateTime.Today.AddMonths(-meses);

                var movimientos = await _context.CajaMovimientos
                    .AsNoTracking()
                    .Where(x => x.EmpresaId == empresaId && x.Activo && x.Fecha >= fechaInicio)
                    .ToListAsync();

                var data = movimientos
                    .GroupBy(x => new { x.Fecha.Year, x.Fecha.Month })
                    .Select(g => new FlujoCajaRealDto
                    {
                        Periodo = $"{g.Key.Month:00}/{g.Key.Year}",
                        Ingresos = g.Where(x => x.Tipo == "Ingreso").Sum(x => x.Monto),
                        Egresos = g.Where(x => x.Tipo == "Egreso").Sum(x => x.Monto),
                        Neto = g.Where(x => x.Tipo == "Ingreso").Sum(x => x.Monto)
                             - g.Where(x => x.Tipo == "Egreso").Sum(x => x.Monto)
                    })
                    .OrderBy(x => x.Periodo)
                    .ToList();

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cash flow real");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
