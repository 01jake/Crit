using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Crit.Server.Data;
using Crit.Shared.Models;

namespace Crit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CuentasPorPagarController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CuentasPorPagarController> _logger;

        public CuentasPorPagarController(ApplicationDbContext context, ILogger<CuentasPorPagarController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CuentaPorPagar>>> GetAll()
        {
            try
            {
                var cuentas = await _context.CuentasPorPagar
                    .Include(x => x.Proveedor)
                    .Include(x => x.Compra)
                    .Include(x => x.Pagos.Where(p => p.Activo))
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
                var cuenta = await _context.CuentasPorPagar
                    .Include(x => x.Proveedor)
                    .Include(x => x.Compra)
                    .Include(x => x.Pagos.Where(p => p.Activo))
                    .FirstOrDefaultAsync(x => x.Id == id);

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
                var cuentas = await _context.CuentasPorPagar
                    .Include(x => x.Proveedor)
                    .Include(x => x.Compra)
                    .Include(x => x.Pagos.Where(p => p.Activo))
                    .Where(x => x.ProveedorId == proveedorId && x.Activa)
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

        [HttpGet("pendientes")]
        public async Task<ActionResult<IEnumerable<CuentaPorPagar>>> GetPendientes()
        {
            try
            {
                var cuentas = await _context.CuentasPorPagar
                    .Include(x => x.Proveedor)
                    .Include(x => x.Compra)
                    .Where(x => x.Activa && x.Saldo > 0)
                    .OrderBy(x => x.FechaVencimiento ?? DateTime.MaxValue)
                    .ToListAsync();

                return Ok(cuentas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuentas por pagar pendientes");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CuentaPorPagar>> Create([FromBody] CuentaPorPagar cuenta)
        {
            try
            {
                cuenta.Id = 0;
                cuenta.TotalPagado = 0m;
                cuenta.FechaUltimoPago = null;
                cuenta.Activa = true;
                cuenta.Estado = CalcularEstado(cuenta.Total, cuenta.TotalPagado, cuenta.FechaVencimiento, cuenta.Activa);

                _context.CuentasPorPagar.Add(cuenta);
                await _context.SaveChangesAsync();

                return Ok(cuenta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cuenta por pagar");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("{id}/registrar-pago")]
        public async Task<ActionResult> RegistrarPago(int id, [FromBody] PagoProveedor pago)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var cuenta = await _context.CuentasPorPagar
                    .Include(x => x.Pagos)
                    .FirstOrDefaultAsync(x => x.Id == id && x.Activa);

                if (cuenta == null)
                    return NotFound("Cuenta por pagar no encontrada");

                if (cuenta.Saldo <= 0)
                    return BadRequest("La cuenta ya está pagada");

                if (pago.Monto <= 0)
                    return BadRequest("El monto debe ser mayor a cero");

                if (pago.Monto > cuenta.Saldo)
                    return BadRequest("El monto no puede ser mayor al saldo pendiente");

                pago.Id = 0;
                pago.CuentaPorPagarId = cuenta.Id;
                pago.FechaPago = pago.FechaPago == default ? DateTime.Now : pago.FechaPago;
                pago.SaldoAnterior = cuenta.Saldo;
                pago.SaldoPosterior = cuenta.Saldo - pago.Monto;
                pago.Activo = true;

                _context.PagosProveedor.Add(pago);

                cuenta.TotalPagado += pago.Monto;
                cuenta.FechaUltimoPago = pago.FechaPago;
                cuenta.Estado = CalcularEstado(cuenta.Total, cuenta.TotalPagado, cuenta.FechaVencimiento, cuenta.Activa);
                var caja = await _context.CajaSesiones
                .OrderByDescending(x => x.FechaApertura)
                .FirstOrDefaultAsync(x => x.Estado == "Abierta");

                if (caja == null)
                    return BadRequest("No hay una caja abierta para registrar el pago al proveedor.");

                var saldoAnteriorCaja = caja.SaldoCalculado;
                var saldoPosteriorCaja = saldoAnteriorCaja - pago.Monto;

                var movimientoCaja = new CajaMovimiento
                {
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
                    UsuarioId = pago.UsuarioId,
                    Activo = true
                };

                _context.CajaMovimientos.Add(movimientoCaja);
                caja.TotalEgresos += pago.Monto;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    cuenta.Id,
                    cuenta.Total,
                    cuenta.TotalPagado,
                    cuenta.Saldo,
                    cuenta.Estado,
                    cuenta.FechaUltimoPago
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al registrar pago en cuenta por pagar {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("{id}/cancelar")]
        public async Task<ActionResult> Cancelar(int id)
        {
            try
            {
                var cuenta = await _context.CuentasPorPagar
                    .Include(x => x.Pagos)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (cuenta == null)
                    return NotFound("Cuenta por pagar no encontrada");

                if (cuenta.Pagos.Any(p => p.Activo))
                    return BadRequest("No se puede cancelar una cuenta que ya tiene pagos registrados.");

                cuenta.Activa = false;
                cuenta.Estado = "Cancelada";

                await _context.SaveChangesAsync();

                return Ok(new { message = "Cuenta por pagar cancelada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar cuenta por pagar {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }
        [HttpGet("{id}/pagos/{pagoId}/comprobante")]
        public async Task<IActionResult> VerComprobantePago(int id, int pagoId)
        {
            try
            {
                var cuenta = await _context.CuentasPorPagar
                    .Include(x => x.Proveedor)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (cuenta == null)
                    return NotFound("Cuenta por pagar no encontrada");

                var pago = await _context.PagosProveedor
                    .FirstOrDefaultAsync(x => x.Id == pagoId && x.CuentaPorPagarId == id && x.Activo);

                if (pago == null)
                    return NotFound("Pago no encontrado");

                var contenido = $@"
            <html>
            <head>
                <title>Comprobante de Pago</title>
                <style>
                    body {{ font-family: Arial, sans-serif; padding: 40px; color: #1f2937; }}
                    .card {{ max-width: 700px; margin: 0 auto; border: 1px solid #e5e7eb; border-radius: 12px; padding: 24px; }}
                    .title {{ font-size: 24px; font-weight: bold; margin-bottom: 20px; }}
                    .row {{ margin-bottom: 10px; }}
                    .label {{ font-weight: bold; }}
                </style>
            </head>
            <body>
                <div class='card'>
                    <div class='title'>Comprobante de Pago a Proveedor</div>
                    <div class='row'><span class='label'>Proveedor:</span> {cuenta.Proveedor?.Nombre}</div>
                    <div class='row'><span class='label'>Folio:</span> {cuenta.FolioFactura}</div>
                    <div class='row'><span class='label'>Fecha de pago:</span> {pago.FechaPago:dd/MM/yyyy HH:mm}</div>
                    <div class='row'><span class='label'>Monto:</span> {pago.Monto:C}</div>
                    <div class='row'><span class='label'>Método:</span> {pago.MetodoPago}</div>
                    <div class='row'><span class='label'>Referencia:</span> {pago.Referencia}</div>
                    <div class='row'><span class='label'>Saldo anterior:</span> {pago.SaldoAnterior:C}</div>
                    <div class='row'><span class='label'>Saldo posterior:</span> {pago.SaldoPosterior:C}</div>
                    <div class='row'><span class='label'>Observaciones:</span> {pago.Observaciones}</div>
                </div>
            </body>
            </html>";

                return Content(contenido, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar comprobante del pago {PagoId}", pagoId);
                return StatusCode(500, "Error interno del servidor");
            }
        }
        private static string CalcularEstado(decimal total, decimal totalPagado, DateTime? fechaVencimiento, bool activa)
        {
            if (!activa)
                return "Cancelada";

            var saldo = total - totalPagado;

            if (saldo <= 0)
                return "Pagada";

            var vencida = fechaVencimiento.HasValue && fechaVencimiento.Value.Date < DateTime.Today;

            if (totalPagado > 0)
                return vencida ? "Vencida" : "Parcial";

            return vencida ? "Vencida" : "Pendiente";
        }
    }
}
