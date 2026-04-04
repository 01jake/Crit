using Crit.Server.Data;
using Crit.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CuentasPorCobrarController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CuentasPorCobrarController> _logger;
        private readonly IEmpresaProvider _empresaProvider;

        public CuentasPorCobrarController(
            ApplicationDbContext context,
            ILogger<CuentasPorCobrarController> logger,
            IEmpresaProvider empresaProvider)
        {
            _context = context;
            _logger = logger;
            _empresaProvider = empresaProvider;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CuentaPorCobrar>>> Get()
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var cuentas = await _context.CuentasPorCobrar
                    .Include(x => x.Cliente)
                    .Include(x => x.Venta)
                    .Include(x => x.Pagos.Where(p => p.Activo))
                    .Where(x => x.EmpresaId == empresaId)
                    .OrderByDescending(x => x.FechaEmision)
                    .ToListAsync();

                return Ok(cuentas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuentas por cobrar");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CuentaPorCobrar>> GetById(int id)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var cuenta = await _context.CuentasPorCobrar
                    .Include(x => x.Cliente)
                    .Include(x => x.Venta)
                    .Include(x => x.Pagos.Where(p => p.Activo))
                    .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);

                if (cuenta == null)
                    return NotFound("Cuenta por cobrar no encontrada");

                return Ok(cuenta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuenta por cobrar {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<CuentaPorCobrar>>> GetByCliente(int clienteId)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var cuentas = await _context.CuentasPorCobrar
                    .Include(x => x.Cliente)
                    .Include(x => x.Venta)
                    .Include(x => x.Pagos.Where(p => p.Activo))
                    .Where(x => x.ClienteId == clienteId && x.EmpresaId == empresaId)
                    .OrderByDescending(x => x.FechaEmision)
                    .ToListAsync();

                return Ok(cuentas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuentas por cobrar del cliente {ClienteId}", clienteId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("{id}/registrar-pago")]
        public async Task<IActionResult> RegistrarPago(int id, [FromBody] PagoCliente pago)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                if (pago.Monto <= 0)
                    return BadRequest("El monto debe ser mayor a cero.");

                var cuenta = await _context.CuentasPorCobrar
                    .Include(x => x.Cliente)
                    .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);

                if (cuenta == null)
                    return NotFound("Cuenta por cobrar no encontrada");

                if (!cuenta.Activa)
                    return BadRequest("La cuenta está cancelada.");

                var saldoActual = cuenta.Total - cuenta.TotalPagado;

                if (pago.Monto > saldoActual)
                    return BadRequest("El pago no puede ser mayor al saldo pendiente.");

                pago.EmpresaId = empresaId;
                pago.CuentaPorCobrarId = cuenta.Id;
                pago.FechaPago = pago.FechaPago == default ? DateTime.Now : pago.FechaPago;
                pago.SaldoAnterior = saldoActual;
                pago.SaldoPosterior = saldoActual - pago.Monto;
                pago.Activo = true;

                _context.Set<PagoCliente>().Add(pago);

                cuenta.TotalPagado += pago.Monto;
                cuenta.FechaUltimoPago = pago.FechaPago;
                cuenta.Estado = CalcularEstado(cuenta.Total, cuenta.TotalPagado, cuenta.FechaVencimiento, cuenta.Activa);

                var caja = await _context.CajaSesiones
                    .OrderByDescending(x => x.FechaApertura)
                    .FirstOrDefaultAsync(x => x.Estado == "Abierta" && x.EmpresaId == empresaId);

                if (caja == null)
                    return BadRequest("No hay una caja abierta para registrar el abono.");

                var saldoAnteriorCaja = caja.SaldoCalculado;
                var saldoPosteriorCaja = saldoAnteriorCaja + pago.Monto;

                var movimientoCaja = new CajaMovimiento
                {
                    EmpresaId = empresaId,
                    CajaSesionId = caja.Id,
                    Fecha = pago.FechaPago,
                    Tipo = "Ingreso",
                    Origen = "AbonoCliente",
                    Monto = pago.Monto,
                    SaldoAnterior = saldoAnteriorCaja,
                    SaldoPosterior = saldoPosteriorCaja,
                    CuentaPorCobrarId = cuenta.Id,
                    Referencia = pago.Referencia,
                    Concepto = $"Abono de cliente a cuenta {cuenta.Folio ?? cuenta.Id.ToString()}",
                    MetodoPago = pago.MetodoPago,
                    Activo = true
                };

                _context.CajaMovimientos.Add(movimientoCaja);
                caja.TotalIngresos += pago.Monto;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(pago);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al registrar pago de cuenta por cobrar {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("{id}/cancelar")]
        public async Task<IActionResult> Cancelar(int id)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var cuenta = await _context.CuentasPorCobrar
                    .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);

                if (cuenta == null)
                    return NotFound("Cuenta por cobrar no encontrada");

                if (cuenta.TotalPagado > 0)
                    return BadRequest("No se puede cancelar una cuenta con pagos registrados.");

                cuenta.Activa = false;
                cuenta.Estado = "Cancelada";

                await _context.SaveChangesAsync();

                return Ok(new { message = "Cuenta cancelada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar cuenta por cobrar {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}/pagos/{pagoId}/comprobante")]
        public async Task<IActionResult> ComprobantePago(int id, int pagoId)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var cuenta = await _context.CuentasPorCobrar
                    .Include(x => x.Cliente)
                    .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);

                if (cuenta == null)
                    return NotFound("Cuenta no encontrada");

                var pago = await _context.Set<PagoCliente>()
                    .FirstOrDefaultAsync(x => x.Id == pagoId && x.CuentaPorCobrarId == id && x.EmpresaId == empresaId && x.Activo);

                if (pago == null)
                    return NotFound("Pago no encontrado");

                var html = $@"
<html>
<head>
    <title>Comprobante de Pago</title>
    <style>
        body {{ font-family: Arial; padding: 30px; }}
        .title {{ font-size: 24px; font-weight: bold; margin-bottom: 20px; }}
        .row {{ margin-bottom: 10px; }}
    </style>
</head>
<body>
    <div class='title'>Comprobante de Pago</div>
    <div class='row'><strong>Cliente:</strong> {cuenta.Cliente?.Nombre}</div>
    <div class='row'><strong>Folio:</strong> {cuenta.Folio}</div>
    <div class='row'><strong>Fecha:</strong> {pago.FechaPago:dd/MM/yyyy HH:mm}</div>
    <div class='row'><strong>Monto:</strong> {pago.Monto:C}</div>
    <div class='row'><strong>Método:</strong> {pago.MetodoPago}</div>
    <div class='row'><strong>Referencia:</strong> {pago.Referencia}</div>
    <div class='row'><strong>Saldo anterior:</strong> {pago.SaldoAnterior:C}</div>
    <div class='row'><strong>Saldo posterior:</strong> {pago.SaldoPosterior:C}</div>
</body>
</html>";

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar comprobante de pago {PagoId}", pagoId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private static string CalcularEstado(decimal total, decimal totalPagado, DateTime? fechaVencimiento, bool activa)
        {
            if (!activa)
                return "Cancelada";

            if (totalPagado >= total)
                return "Pagada";

            if (totalPagado > 0)
                return "Parcial";

            if (fechaVencimiento.HasValue && fechaVencimiento.Value.Date < DateTime.Today)
                return "Vencida";

            return "Pendiente";
        }
    }
}
