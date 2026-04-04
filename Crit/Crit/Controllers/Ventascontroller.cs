using Crit.Server.Data;
using Crit.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Crit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VentasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VentasController> _logger;
        private readonly IEmpresaProvider _empresaProvider;

        public VentasController(
            ApplicationDbContext context,
            ILogger<VentasController> logger,
            IEmpresaProvider empresaProvider)
        {
            _context = context;
            _logger = logger;
            _empresaProvider = empresaProvider;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Venta>>> GetVentas()
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var ventas = await _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Where(v => v.EmpresaId == empresaId)
                    .OrderByDescending(v => v.Fecha)
                    .ToListAsync();

                return Ok(ventas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Venta>> GetVenta(int id)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var venta = await _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(v => v.Id == id && v.EmpresaId == empresaId);

                if (venta == null)
                    return NotFound();

                return Ok(venta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener venta {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<Venta>>> GetVentasPorCliente(int clienteId)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var ventas = await _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.Detalles)
                    .Where(v => v.ClienteId == clienteId && v.EmpresaId == empresaId)
                    .OrderByDescending(v => v.Fecha)
                    .ToListAsync();

                return Ok(ventas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas del cliente {ClienteId}", clienteId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("recientes")]
        public async Task<ActionResult<IEnumerable<Venta>>> GetVentasRecientes([FromQuery] int cantidad = 10)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var ventas = await _context.Ventas
                    .Include(v => v.Cliente)
                    .Where(v => v.EmpresaId == empresaId)
                    .OrderByDescending(v => v.Fecha)
                    .Take(cantidad)
                    .ToListAsync();

                return Ok(ventas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas recientes");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateVenta(Venta venta)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                venta.EmpresaId = empresaId;
                venta.Fecha = DateTime.Now;

                foreach (var d in venta.Detalles)
                {
                    var producto = await _context.Productos
                        .FirstOrDefaultAsync(p => p.Id == d.ProductoId && p.EmpresaId == empresaId);

                    if (producto == null)
                        return BadRequest($"Producto con ID {d.ProductoId} no encontrado");

                    d.Subtotal = d.Cantidad * d.PrecioUnitario;
                }

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();

                if (venta.EsCredito)
                {
                    var cuentaPorCobrar = new CuentaPorCobrar
                    {
                        EmpresaId = empresaId,
                        ClienteId = venta.ClienteId,
                        VentaId = venta.Id,
                        Folio = venta.NumeroVenta,
                        FechaEmision = venta.Fecha,
                        FechaVencimiento = venta.Fecha.AddDays(venta.DiasCredito ?? 30),
                        Subtotal = venta.Subtotal,
                        Descuento = venta.Descuento,
                        IVA = venta.IVA,
                        Total = venta.Total,
                        TotalPagado = 0m,
                        Estado = "Pendiente",
                        Observaciones = "Generada automáticamente desde venta a crédito",
                        Activa = true
                    };

                    _context.CuentasPorCobrar.Add(cuentaPorCobrar);
                    await _context.SaveChangesAsync();
                }

                if (!venta.EsCredito)
                {
                    var caja = await _context.CajaSesiones
                        .FirstOrDefaultAsync(x => x.Estado == "Abierta" && x.EmpresaId == empresaId);

                    if (caja == null)
                        return BadRequest("No hay una caja abierta para registrar la venta de contado.");

                    var saldoAnterior = caja.SaldoCalculado;
                    var saldoPosterior = saldoAnterior + venta.Total;

                    var movimientoCaja = new CajaMovimiento
                    {
                        EmpresaId = empresaId,
                        CajaSesionId = caja.Id,
                        Fecha = venta.Fecha,
                        Tipo = "Ingreso",
                        Origen = "VentaContado",
                        Monto = venta.Total,
                        SaldoAnterior = saldoAnterior,
                        SaldoPosterior = saldoPosterior,
                        VentaId = venta.Id,
                        Referencia = venta.NumeroVenta,
                        Concepto = $"Venta de contado {venta.NumeroVenta}",
                        MetodoPago = venta.FormaPago,
                        Activo = true
                    };

                    _context.CajaMovimientos.Add(movimientoCaja);
                    caja.TotalIngresos += venta.Total;

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return Ok(venta);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear venta");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("fecha")]
        public async Task<ActionResult<IEnumerable<Venta>>> GetVentasPorFecha(
            [FromQuery] DateTime desde,
            [FromQuery] DateTime hasta)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var ventas = await _context.Ventas
                    .Include(v => v.Cliente)
                    .Where(v => v.EmpresaId == empresaId && v.Fecha >= desde && v.Fecha <= hasta)
                    .OrderByDescending(v => v.Fecha)
                    .ToListAsync();

                return Ok(ventas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas por fecha");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("total-mes")]
        public async Task<ActionResult<decimal>> GetTotalVentasMes([FromQuery] int mes, [FromQuery] int año)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var total = await _context.Ventas
                    .Where(v => v.EmpresaId == empresaId &&
                                v.Fecha.Month == mes &&
                                v.Fecha.Year == año &&
                                v.Estado == "Completada")
                    .SumAsync(v => v.Total);

                return Ok(total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener total de ventas del mes");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> DescargarVentaPdf(int id)
        {
            try
            {
                var empresaId = await _empresaProvider.GetEmpresaIdAsync();

                if (empresaId <= 0)
                    return Unauthorized("No se pudo determinar la empresa del usuario.");

                var venta = await _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(v => v.Id == id && v.EmpresaId == empresaId);

                if (venta == null)
                    return NotFound($"Venta {id} no encontrada");

                var pdfBytes = GenerarPdfVenta(venta);

                return File(pdfBytes, "application/pdf", $"Venta-{venta.NumeroVenta}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de venta {Id}", id);
                return StatusCode(500, "Error al generar el PDF");
            }
        }

        private byte[] GenerarPdfVenta(Venta venta)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var colorPrimario = Color.FromHex("#1F2A44");
            var colorGrisClaro = Color.FromHex("#F8F9FA");
            var colorTextoSuave = Color.FromHex("#4B5563");
            var colorBorde = Color.FromHex("#E5E7EB");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Helvetica"));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("POLYNEX").FontSize(24).Bold().FontColor(colorPrimario);
                            col.Item().Text("TU NOMBRE O RAZÓN SOCIAL").FontSize(10).SemiBold();
                            col.Item().Text("RFC: XXXX000000XXX").FontSize(9);
                            col.Item().Text("Calle Ejemplo #123, Col. Centro").FontSize(9);
                            col.Item().Text("Hermosillo, Sonora, CP: 83000").FontSize(9);
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Background(colorPrimario).Padding(5).Text("NOTA DE VENTA / PRE-FACTURA").FontColor(Colors.White).Bold().AlignCenter();
                            col.Item().Border(1).BorderColor(colorPrimario).Padding(5).AlignCenter().Column(innerCol =>
                            {
                                innerCol.Item().Text("FOLIO").FontSize(8).Bold();
                                innerCol.Item().Text($"{venta.NumeroVenta}").FontSize(14).Bold().FontColor(colorPrimario);
                            });
                            col.Item().PaddingTop(5).Text($"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm}").FontSize(9);
                        });
                    });

                    page.Content().PaddingVertical(20).Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Border(1).BorderColor(colorBorde).Padding(10).Column(col =>
                            {
                                col.Item().Text("RECEPTOR (CLIENTE)").FontSize(8).Bold().FontColor(colorPrimario);
                                col.Item().Text(venta.Cliente?.Nombre ?? "PÚBLICO EN GENERAL").FontSize(11).Bold();
                                col.Item().Text($"RFC: {(string.IsNullOrEmpty(venta.Cliente?.RFC) ? "XAXX010101000" : venta.Cliente.RFC.ToUpper())}").FontSize(9);

                                if (!string.IsNullOrEmpty(venta.Cliente?.RegimenFiscal))
                                    col.Item().Text($"Régimen: {venta.Cliente.RegimenFiscal}").FontSize(8);

                                col.Item().Text($"CP: {venta.Cliente?.CodigoPostal ?? "S/N"}").FontSize(9);
                                col.Item().Text($"Email: {venta.Cliente?.Email ?? "N/A"}").FontSize(9).FontColor(colorTextoSuave);
                            });

                            row.ConstantItem(15);

                            row.RelativeItem().Border(1).BorderColor(colorBorde).Padding(10).Column(col =>
                            {
                                col.Item().Text("DETALLES DE PAGO / FISCALES").FontSize(8).Bold().FontColor(colorPrimario);
                                col.Item().Text($"Método: {(string.IsNullOrEmpty(venta.MetodoPago) ? "PUE - Pago en una sola exhibición" : venta.MetodoPago)}").FontSize(9);
                                col.Item().Text($"Forma: {(string.IsNullOrEmpty(venta.FormaPago) ? "03 - Transferencia electrónica" : venta.FormaPago)}").FontSize(9);

                                string usoCfdiFinal = !string.IsNullOrEmpty(venta.UsoCFDI)
                                    ? venta.UsoCFDI
                                    : (!string.IsNullOrEmpty(venta.Cliente?.UsoCFDI) ? venta.Cliente.UsoCFDI : "G03 - Gastos en general");

                                col.Item().Text($"Uso CFDI: {usoCfdiFinal}").FontSize(9);
                                col.Item().Text("Moneda: MXN - Peso Mexicano").FontSize(9);
                            });
                        });

                        column.Item().PaddingVertical(15);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1.5f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("CANT.");
                                header.Cell().Element(CellStyle).Text("DESCRIPCIÓN / PRODUCTO");
                                header.Cell().Element(CellStyle).AlignRight().Text("P. UNITARIO");
                                header.Cell().Element(CellStyle).AlignRight().Text("IMPORTE");

                                static IContainer CellStyle(IContainer container) =>
                                    container.DefaultTextStyle(x => x.SemiBold().FontColor(Colors.White))
                                        .Background("#3c3c3c").PaddingVertical(5).PaddingHorizontal(5);
                            });

                            foreach (var detalle in venta.Detalles)
                            {
                                table.Cell().Element(RowStyle).AlignCenter().Text(detalle.Cantidad.ToString());
                                table.Cell().Element(RowStyle).Text(detalle.Producto?.Nombre ?? "Sin nombre");
                                table.Cell().Element(RowStyle).AlignRight().Text($"${detalle.PrecioUnitario:N2}");
                                table.Cell().Element(RowStyle).AlignRight().Text($"${detalle.Subtotal:N2}");

                                static IContainer RowStyle(IContainer container) =>
                                    container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(8).PaddingHorizontal(5);
                            }
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem();

                            row.ConstantItem(200).PaddingTop(10).Column(col =>
                            {
                                col.Item().Row(r =>
                                {
                                    r.RelativeItem().Text("SUBTOTAL").FontSize(9);
                                    r.RelativeItem().AlignRight().Text($"${venta.Subtotal:N2}");
                                });
                                col.Item().Row(r =>
                                {
                                    r.RelativeItem().Text("IVA (16%)").FontSize(9);
                                    r.RelativeItem().AlignRight().Text($"${venta.IVA:N2}");
                                });
                                col.Item().PaddingTop(5).Background(colorGrisClaro).Padding(5).Row(r =>
                                {
                                    r.RelativeItem().Text("TOTAL").Bold().FontSize(12).FontColor(colorPrimario);
                                    r.RelativeItem().AlignRight().Text($"${venta.Total:N2}").Bold().FontSize(12).FontColor(colorPrimario);
                                });
                            });
                        });

                        column.Item().PaddingTop(40).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("ESTE DOCUMENTO NO ES UN COMPROBANTE FISCAL (CFDI)").FontSize(8).Bold().FontColor(Colors.Red.Medium);
                                col.Item().Text("Favor de solicitar su factura en un lapso no mayor a 72 horas.").FontSize(8);
                                col.Item().PaddingTop(10).Text("Cuentas Bancarias:").FontSize(8).Bold();
                                col.Item().Text("Banco Ejemplo - CLABE: 0123 4567 8901 2345 67").FontSize(8);
                            });

                            row.ConstantItem(150).Column(col =>
                            {
                                col.Item().PaddingTop(20).LineHorizontal(0.5f);
                                col.Item().AlignCenter().Text("Firma de Recibido").FontSize(8);
                            });
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
