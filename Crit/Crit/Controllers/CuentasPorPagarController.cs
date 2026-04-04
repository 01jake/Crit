using Crit.Server.Data;
using Crit.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CuentasPorPagarController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CuentasPorPagarController> _logger;
        private readonly IEmpresaProvider _empresaProvider;

        public CuentasPorPagarController(
            ApplicationDbContext context,
            ILogger<CuentasPorPagarController> logger,
            IEmpresaProvider empresaProvider)
        {
            _context = context;
            _logger = logger;
            _empresaProvider = empresaProvider;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CuentaPorPagar>>> Get()
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var cuentas = await _context.CuentasPorPagar
                    .Include(x => x.Proveedor)
                    .Include(x => x.Compra)
                    .Include(x => x.Pagos.Where(p => p.Activo))
                    .Where(x => x.EmpresaId == empresaId)
                    .OrderByDescending(x => x.FechaEmision)
                    .ToListAsync();

                return Ok(cuentas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuentas por pagar");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CuentaPorPagar>> GetById(int id)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var cuenta = await _context.CuentasPorPagar
                    .Include(x => x.Proveedor)
                    .Include(x => x.Compra)
                    .Include(x => x.Pagos.Where(p => p.Activo))
                    .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);

                if (cuenta == null)
                    return NotFound("Cuenta por pagar no encontrada");

                return Ok(cuenta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuenta por pagar {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("proveedor/{proveedorId}")]
        public async Task<ActionResult<IEnumerable<CuentaPorPagar>>> GetByProveedor(int proveedorId)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var cuentas = await _context.CuentasPorPagar
                    .Include(x => x.Proveedor)
                    .Include(x => x.Compra)
                    .Include(x => x.Pagos.Where(p => p.Activo))
                    .Where(x => x.ProveedorId == proveedorId && x.EmpresaId == empresaId)
                    .OrderByDescending(x => x.FechaEmision)
                    .ToListAsync();

                return Ok(cuentas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuentas por pagar del proveedor {ProveedorId}", proveedorId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("{id}/registrar-pago")]
        public async Task<IActionResult> RegistrarPago(int id, [FromBody] PagoProveedor pago)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                if (pago.Monto <= 0)
                    return BadRequest("El monto debe ser mayor a cero.");

                var cuenta = await _context.CuentasPorPagar
                    .Include(x => x.Proveedor)
                    .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);

                if (cuenta == null)
                    return NotFound("Cuenta por pagar no encontrada");

                if (!cuenta.Activa)
                    return BadRequest("La cuenta está cancelada.");

                var saldoActual = cuenta.Total - cuenta.TotalPagado;

                if (pago.Monto > saldoActual)
                    return BadRequest("El pago no puede ser mayor al saldo pendiente.");

                pago.EmpresaId = empresaId;
                pago.CuentaPorPagarId = cuenta.Id;
                pago.FechaPago = pago.FechaPago == default ? DateTime.Now : pago.FechaPago;
                pago.SaldoAnterior = saldoActual;
                pago.SaldoPosterior = saldoActual - pago.Monto;
                pago.Activo = true;

                _context.Set<PagoProveedor>().Add(pago);

                cuenta.TotalPagado += pago.Monto;
                cuenta.FechaUltimoPago = pago.FechaPago;
                cuenta.Estado = CalcularEstado(cuenta.Total, cuenta.TotalPagado, cuenta.FechaVencimiento, cuenta.Activa);

                var caja = await _context.CajaSesiones
                    .OrderByDescending(x => x.FechaApertura)
                    .FirstOrDefaultAsync(x => x.Estado == "Abierta" && x.EmpresaId == empresaId);

                if (caja == null)
                    return BadRequest("No hay una caja abierta para registrar el pago al proveedor.");

                var saldoAnteriorCaja = caja.SaldoCalculado;
                var saldoPosteriorCaja = saldoAnteriorCaja - pago.Monto;

                var movimientoCaja = new CajaMovimiento
                {
                    EmpresaId = empresaId,
                    CajaSesionId = caja.Id,
                    Fecha = pago.FechaPago,
                    Tipo = "Egreso",
                    Origen = "PagoProveedor",
                    Monto = pago.Monto,
                    SaldoAnterior = saldoAnteriorCaja,
                    SaldoPosterior = saldoPosteriorCaja,
                    CuentaPorPagarId = cuenta.Id,
                    Referencia = pago.Referencia,
                    Concepto = $"Pago a proveedor de cuenta {cuenta.FolioFactura ?? cuenta.Id.ToString()}",
                    MetodoPago = pago.MetodoPago,
                    Activo = true
                };

                _context.CajaMovimientos.Add(movimientoCaja);
                caja.TotalEgresos += pago.Monto;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(pago);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al registrar pago de cuenta por pagar {Id}", id);
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

                var cuenta = await _context.CuentasPorPagar
                    .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);

                if (cuenta == null)
                    return NotFound("Cuenta por pagar no encontrada");

                if (cuenta.TotalPagado > 0)
                    return BadRequest("No se puede cancelar una cuenta con pagos registrados.");

                cuenta.Activa = false;
                cuenta.Estado = "Cancelada";

                await _context.SaveChangesAsync();

                return Ok(new { message = "Cuenta cancelada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar cuenta por pagar {Id}", id);
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

                var cuenta = await _context.CuentasPorPagar
                    .Include(x => x.Proveedor)
                    .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);

                if (cuenta == null)
                    return NotFound("Cuenta no encontrada");

                var pago = await _context.Set<PagoProveedor>()
                    .FirstOrDefaultAsync(x => x.Id == pagoId && x.CuentaPorPagarId == id && x.EmpresaId == empresaId && x.Activo);

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
    <div class='title'>Comprobante de Pago a Proveedor</div>
    <div class='row'><strong>Proveedor:</strong> {cuenta.Proveedor?.Nombre}</div>
    <div class='row'><strong>Folio:</strong> {cuenta.FolioFactura}</div>
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
                _logger.LogError(ex, "Error al generar comprobante de pago proveedor {PagoId}", pagoId);
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
