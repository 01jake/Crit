using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public VentasController(ApplicationDbContext context, ILogger<VentasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/ventas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Venta>>> GetVentas()
        {
            try
            {
                var ventas = await _context.Ventas
                    .Include(v => v.Cliente)
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

        // GET: api/ventas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Venta>> GetVenta(int id)
        {
            try
            {
                var venta = await _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (venta == null)
                {
                    return NotFound($"Venta con ID {id} no encontrada");
                }

                return Ok(venta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener venta {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/ventas/cliente/5
        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<Venta>>> GetVentasPorCliente(int clienteId)
        {
            try
            {
                var ventas = await _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.Detalles)
                    .Where(v => v.ClienteId == clienteId)
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

        // GET: api/ventas/recientes?cantidad=10
        [HttpGet("recientes")]
        public async Task<ActionResult<IEnumerable<Venta>>> GetVentasRecientes([FromQuery] int cantidad = 10)
        {
            try
            {
                var ventas = await _context.Ventas
                    .Include(v => v.Cliente)
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

        // POST: api/ventas
        [HttpPost]
        public async Task<ActionResult<Venta>> CreateVenta([FromBody] Venta venta)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("=== INICIO: Creando venta ===");

                // Validaciones
                if (venta.ClienteId == 0)
                {
                    _logger.LogWarning("ClienteId es 0");
                    return BadRequest("Debe seleccionar un cliente");
                }

                if (venta.Detalles == null || !venta.Detalles.Any())
                {
                    _logger.LogWarning("No hay detalles en la venta");
                    return BadRequest("La venta debe tener al menos un producto");
                }

                _logger.LogInformation($"Venta con {venta.Detalles.Count} productos");

                // ✅ Limpiar TODAS las navegaciones que vienen del cliente
                venta.Cliente = null;
                venta.Id = 0;

                foreach (var detalle in venta.Detalles)
                {
                    detalle.Venta = null;
                    detalle.Producto = null;
                    detalle.VentaId = 0;
                    detalle.Id = 0;

                    _logger.LogInformation($"Detalle: ProductoId={detalle.ProductoId}, Cantidad={detalle.Cantidad}");
                }

                // Generar número de venta
                var ultimaVenta = await _context.Ventas
                    .AsNoTracking()
                    .OrderByDescending(v => v.Id)
                    .FirstOrDefaultAsync();

                var numero = ultimaVenta != null ? ultimaVenta.Id + 1 : 1;
                venta.NumeroVenta = $"V-{DateTime.Now:yyyyMMdd}-{numero:D6}";
                venta.Fecha = DateTime.Now;

                _logger.LogInformation($"Número de venta generado: {venta.NumeroVenta}");

                // Validar y descontar stock
                foreach (var detalle in venta.Detalles)
                {
                    var producto = await _context.Productos.FindAsync(detalle.ProductoId);

                    if (producto == null)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError($"Producto {detalle.ProductoId} no encontrado");
                        return BadRequest($"Producto con ID {detalle.ProductoId} no encontrado");
                    }

                    if (producto.Stock < detalle.Cantidad)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError($"Stock insuficiente para {producto.Nombre}");
                        return BadRequest($"Stock insuficiente para {producto.Nombre}. Disponible: {producto.Stock}");
                    }

                    _logger.LogInformation($"Descontando {detalle.Cantidad} de {producto.Nombre}. Stock actual: {producto.Stock}");

                    producto.Stock -= detalle.Cantidad;
                }

                // Agregar la venta
                _context.Ventas.Add(venta);

                _logger.LogInformation("Guardando cambios en la base de datos...");

                await _context.SaveChangesAsync();
                if (!venta.EsCredito)
                {
                    var caja = await _context.CajaSesiones
                        .OrderByDescending(x => x.FechaApertura)
                        .FirstOrDefaultAsync(x => x.Estado == "Abierta");

                    if (caja == null)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest("No hay una caja abierta para registrar la venta de contado.");
                    }

                    var saldoAnterior = caja.SaldoCalculado;
                    var saldoPosterior = saldoAnterior + venta.Total;

                    var movimientoCaja = new CajaMovimiento
                    {
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
                        UsuarioId = venta.UsuarioId,
                        Activo = true
                    };

                    _context.CajaMovimientos.Add(movimientoCaja);
                    caja.TotalIngresos += venta.Total;

                    await _context.SaveChangesAsync();
                }
                await transaction.CommitAsync();

                _logger.LogInformation($"✅ Venta creada exitosamente con ID: {venta.Id}");
               

                // ✅ Retornar solo datos básicos sin navegaciones complejas
                return Ok(new
                {
                    Id = venta.Id,
                    NumeroVenta = venta.NumeroVenta,
                    ClienteId = venta.ClienteId,
                    Fecha = venta.Fecha,
                    Subtotal = venta.Subtotal,
                    Descuento = venta.Descuento,
                    IVA = venta.IVA,
                    Total = venta.Total,
                    Estado = venta.Estado,
                    Notas = venta.Notas,
                    EsCredito = venta.EsCredito
                });
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                _logger.LogError(dbEx, "Error de base de datos al crear venta");
                _logger.LogError($"Inner: {dbEx.InnerException?.Message}");
                return StatusCode(500, new
                {
                    error = "Error de base de datos",
                    message = dbEx.InnerException?.Message ?? dbEx.Message
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear venta");
                _logger.LogError($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    error = "Error interno",
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        // GET: api/ventas/fecha
        [HttpGet("fecha")]
        public async Task<ActionResult<IEnumerable<Venta>>> GetVentasPorFecha(
            [FromQuery] DateTime desde,
            [FromQuery] DateTime hasta)
        {
            try
            {
                var ventas = await _context.Ventas
                    .Include(v => v.Cliente)
                    .Where(v => v.Fecha >= desde && v.Fecha <= hasta)
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

        // GET: api/ventas/total-mes?mes=1&año=2024
        [HttpGet("total-mes")]
        public async Task<ActionResult<decimal>> GetTotalVentasMes([FromQuery] int mes, [FromQuery] int año)
        {
            try
            {
                var total = await _context.Ventas
                    .Where(v => v.Fecha.Month == mes && v.Fecha.Year == año && v.Estado == "Completada")
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
                var venta = await _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (venta == null)
                {
                    return NotFound($"Venta {id} no encontrada");
                }

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

            // Definición de Colores e Identidad
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
                    // --- CABECERA (EMISOR) ---
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

                    // --- DATOS DEL CLIENTE (RECEPTOR) ---
                    page.Content().PaddingVertical(20).Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            // --- BLOQUE IZQUIERDO: DATOS DEL CLIENTE ---
                            row.RelativeItem().Border(1).BorderColor(colorBorde).Padding(10).Column(col =>
                            {
                                col.Item().Text("RECEPTOR (CLIENTE)").FontSize(8).Bold().FontColor(colorPrimario);

                                // Nombre del cliente
                                col.Item().Text(venta.Cliente?.Nombre ?? "PÚBLICO EN GENERAL").FontSize(11).Bold();

                                // RFC
                                col.Item().Text($"RFC: {(string.IsNullOrEmpty(venta.Cliente?.RFC) ? "XAXX010101000" : venta.Cliente.RFC.ToUpper())}").FontSize(9);

                                // Régimen Fiscal (Nuevo campo)
                                if (!string.IsNullOrEmpty(venta.Cliente?.RegimenFiscal))
                                {
                                    col.Item().Text($"Régimen: {venta.Cliente.RegimenFiscal}").FontSize(8);
                                }

                                // Código Postal
                                col.Item().Text($"CP: {venta.Cliente?.CodigoPostal ?? "S/N"}").FontSize(9);

                                // Email
                                col.Item().Text($"Email: {venta.Cliente?.Email ?? "N/A"}").FontSize(9).FontColor(colorTextoSuave);
                            });

                            row.ConstantItem(15);

                            // --- BLOQUE DERECHO: DETALLES FISCALES DE LA OPERACIÓN ---
                            row.RelativeItem().Border(1).BorderColor(colorBorde).Padding(10).Column(col =>
                            {
                                col.Item().Text("DETALLES DE PAGO / FISCALES").FontSize(8).Bold().FontColor(colorPrimario);

                                // Método de Pago (Asegúrate de que venta.MetodoPago exista en el modelo)
                                col.Item().Text($"Método: {(string.IsNullOrEmpty(venta.MetodoPago) ? "PUE - Pago en una sola exhibición" : venta.MetodoPago)}").FontSize(9);

                                // Forma de Pago
                                col.Item().Text($"Forma: {(string.IsNullOrEmpty(venta.FormaPago) ? "03 - Transferencia electrónica" : venta.FormaPago)}").FontSize(9);

                                // Uso de CFDI corregido
                                string usoCfdiFinal = !string.IsNullOrEmpty(venta.UsoCFDI) ? venta.UsoCFDI :
                                                     (!string.IsNullOrEmpty(venta.Cliente?.UsoCFDI) ? venta.Cliente.UsoCFDI : "G03 - Gastos en general");

                                col.Item().Text($"Uso CFDI: {usoCfdiFinal}").FontSize(9);

                                col.Item().Text("Moneda: MXN - Peso Mexicano").FontSize(9);
                            });
                        });

                        column.Item().PaddingVertical(15);

                        // --- TABLA DE CONCEPTOS ---
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1); // Cantidad
                                columns.RelativeColumn(4); // Descripción
                                columns.RelativeColumn(1.5f); // Precio Unitario
                                columns.RelativeColumn(1.5f); // Importe
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

                        // --- TOTALES ---
                        column.Item().Row(row =>
                        {
                            row.RelativeItem(); // Espacio vacío a la izquierda

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

                        // --- TEXTO LEGAL Y FIRMA ---
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