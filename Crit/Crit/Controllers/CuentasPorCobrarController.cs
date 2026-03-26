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
    public class CuentasPorCobrarController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CuentasPorCobrarController> _logger;

        public CuentasPorCobrarController(ApplicationDbContext context, ILogger<CuentasPorCobrarController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CuentaPorCobrar>>> GetAll()
        {
            try
            {
                var cuentas = await _context.CuentasPorCobrar
                    .Include(x => x.Cliente)
                    .Include(x => x.Venta)
                    .Include(x => x.Pagos.Where(p => p.Activo))
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
                var cuenta = await _context.CuentasPorCobrar
                    .Include(x => x.Cliente)
                    .Include(x => x.Venta)
                    .Include(x => x.Pagos.Where(p => p.Activo))
                    .FirstOrDefaultAsync(x => x.Id == id);

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
                var cuentas = await _context.CuentasPorCobrar
                    .Include(x => x.Cliente)
                    .Include(x => x.Venta)
                    .Include(x => x.Pagos.Where(p => p.Activo))
                    .Where(x => x.ClienteId == clienteId && x.Activa)
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

        [HttpGet("pendientes")]
        public async Task<ActionResult<IEnumerable<CuentaPorCobrar>>> GetPendientes()
        {
            try
            {
                var cuentas = await _context.CuentasPorCobrar
                    .Include(x => x.Cliente)
                    .Include(x => x.Venta)
                    .Where(x => x.Activa && x.Saldo > 0)
                    .OrderBy(x => x.FechaVencimiento ?? DateTime.MaxValue)
                    .ToListAsync();

                return Ok(cuentas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuentas por cobrar pendientes");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CuentaPorCobrar>> Create([FromBody] CuentaPorCobrar cuenta)
        {
            try
            {
                cuenta.Id = 0;
                cuenta.TotalPagado = 0m;
                cuenta.FechaUltimoPago = null;
                cuenta.Activa = true;
                cuenta.Estado = CalcularEstado(cuenta.Total, cuenta.TotalPagado, cuenta.FechaVencimiento, cuenta.Activa);

                _context.CuentasPorCobrar.Add(cuenta);
                await _context.SaveChangesAsync();

                return Ok(cuenta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cuenta por cobrar");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("{id}/registrar-pago")]
        public async Task<ActionResult> RegistrarPago(int id, [FromBody] PagoCliente pago)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var cuenta = await _context.CuentasPorCobrar
                    .Include(x => x.Pagos)
                    .FirstOrDefaultAsync(x => x.Id == id && x.Activa);

                if (cuenta == null)
                    return NotFound("Cuenta por cobrar no encontrada");

                if (cuenta.Saldo <= 0)
                    return BadRequest("La cuenta ya está pagada");

                if (pago.Monto <= 0)
                    return BadRequest("El monto debe ser mayor a cero");

                if (pago.Monto > cuenta.Saldo)
                    return BadRequest("El monto no puede ser mayor al saldo pendiente");

                pago.Id = 0;
                pago.CuentaPorCobrarId = cuenta.Id;
                pago.FechaPago = pago.FechaPago == default ? DateTime.Now : pago.FechaPago;
                pago.SaldoAnterior = cuenta.Saldo;
                pago.SaldoPosterior = cuenta.Saldo - pago.Monto;
                pago.Activo = true;

                _context.PagosCliente.Add(pago);

                cuenta.TotalPagado += pago.Monto;
                cuenta.FechaUltimoPago = pago.FechaPago;
                cuenta.Estado = CalcularEstado(cuenta.Total, cuenta.TotalPagado, cuenta.FechaVencimiento, cuenta.Activa);

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
                _logger.LogError(ex, "Error al registrar pago en cuenta por cobrar {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("{id}/cancelar")]
        public async Task<ActionResult> Cancelar(int id)
        {
            try
            {
                var cuenta = await _context.CuentasPorCobrar.FirstOrDefaultAsync(x => x.Id == id);

                if (cuenta == null)
                    return NotFound("Cuenta por cobrar no encontrada");

                cuenta.Activa = false;
                cuenta.Estado = "Cancelada";

                await _context.SaveChangesAsync();

                return Ok(new { message = "Cuenta por cobrar cancelada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar cuenta por cobrar {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }
        [HttpGet("{id}/pagos/{pagoId}/comprobante")]
        public async Task<IActionResult> VerComprobantePago(int id, int pagoId)
        {
            try
            {
                var cuenta = await _context.CuentasPorCobrar
                    .Include(x => x.Cliente)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (cuenta == null)
                    return NotFound("Cuenta por cobrar no encontrada");

                var pago = await _context.PagosCliente
                    .FirstOrDefaultAsync(x => x.Id == pagoId && x.CuentaPorCobrarId == id && x.Activo);

                if (pago == null)
                    return NotFound("Pago no encontrado");

                var contenido = $@"
        <html>
        <head>
            <title>Comprobante de Abono</title>
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
                <div class='title'>Comprobante de Abono de Cliente</div>
                <div class='row'><span class='label'>Cliente:</span> {cuenta.Cliente?.Nombre ?? "-"}</div>
                <div class='row'><span class='label'>Folio:</span> {cuenta.Folio ?? "-"}</div>
                <div class='row'><span class='label'>Fecha de pago:</span> {pago.FechaPago:dd/MM/yyyy HH:mm}</div>
                <div class='row'><span class='label'>Monto:</span> {pago.Monto:C}</div>
                <div class='row'><span class='label'>Método:</span> {pago.MetodoPago ?? "-"}</div>
                <div class='row'><span class='label'>Referencia:</span> {pago.Referencia ?? "-"}</div>
                <div class='row'><span class='label'>Saldo anterior:</span> {pago.SaldoAnterior:C}</div>
                <div class='row'><span class='label'>Saldo posterior:</span> {pago.SaldoPosterior:C}</div>
                <div class='row'><span class='label'>Observaciones:</span> {pago.Observaciones ?? "-"}</div>
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
