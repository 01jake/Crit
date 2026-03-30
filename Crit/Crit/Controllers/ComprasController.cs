using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crit.Server.Data;
using Crit.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;


namespace Crit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComprasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ComprasController> _logger;

        public ComprasController(ApplicationDbContext context, ILogger<ComprasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/compras
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Compra>>> GetCompras()
        {
            try
            {
                var compras = await _context.Compra
                    .Include(c => c.Proveedor)
                    .Include(c => c.Detalles)
                        .ThenInclude(d => d.Producto)
                    .OrderByDescending(c => c.Fecha)
                    .ToListAsync();

                return Ok(compras);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener compras");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/compras
        [HttpPost]
        public async Task<IActionResult> CrearCompra(Compra compra)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                compra.Fecha = DateTime.Now;

                decimal total = 0;

                foreach (var d in compra.Detalles)
                {
                    var producto = await _context.Productos.FindAsync(d.ProductoId);

                    if (producto == null)
                        return BadRequest("Producto no existe");

                    int stockAnterior = producto.Stock;

                    // 🔥 REGLA #1: recalcular SIEMPRE
                    d.Subtotal = d.Cantidad * d.PrecioUnitario;

                    // 🔥 INVENTARIO
                    producto.Stock += d.Cantidad;

                    // 🔥 COSTO PROMEDIO
                    producto.PrecioCompra =
                        ((producto.PrecioCompra * stockAnterior) +
                        (d.PrecioUnitario * d.Cantidad))
                        / (stockAnterior + d.Cantidad);

                    // 🔥 KARDEX
                    //_context.Kardex.Add(new Kardex
                    //{
                    //    ProductoId = producto.Id,
                    //    Fecha = DateTime.Now,
                    //    TipoMovimiento = "COMPRA",
                    //    Cantidad = d.Cantidad,
                    //    CostoUnitario = d.PrecioUnitario,
                    //    StockAnterior = stockAnterior,
                    //    StockNuevo = producto.Stock
                    //});

                    total += d.Subtotal;
                }
                compra.Subtotal = total;
                compra.IVA = total * 0.16m;
                compra.Total = compra.Subtotal + compra.IVA;

                _context.Compra.Add(compra);
                await _context.SaveChangesAsync();
                if (compra.EsCredito)
                {
                    var cuentaPorPagar = new CuentaPorPagar
                    {
                        ProveedorId = compra.ProveedorId,
                        CompraId = compra.Id,
                        FolioFactura = string.IsNullOrWhiteSpace(compra.FolioFactura)
                            ? $"{compra.SerieFactura}-{compra.Id}"
                            : compra.FolioFactura,
                        FechaEmision = compra.Fecha,
                        FechaVencimiento = compra.EsCredito
                        ? compra.Fecha.AddDays(compra.DiasCredito ?? 30)
                        : null,
                        Subtotal = compra.Subtotal,
                        Descuento = 0m,
                        IVA = compra.IVA,
                        Total = compra.Total,
                        TotalPagado = 0m,
                        Estado = "Pendiente",
                        Observaciones = "Generada automáticamente desde compra a crédito",
                        Activa = true
                    };

                    _context.CuentasPorPagar.Add(cuentaPorPagar);
                    await _context.SaveChangesAsync();
                }
                await transaction.CommitAsync();

                return Ok(compra);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("historial")]
        public async Task<IActionResult> Historial()
        {
            var data = await _context.Compra
                .Include(c => c.Proveedor)
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();

            return Ok(data);
        }
        // DELETE: api/compras/5 (rollback stock)
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelarCompra(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var compra = await _context.Compra
                    .Include(c => c.Detalles)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (compra == null)
                    return NotFound();

                foreach (var d in compra.Detalles)
                {
                    var producto = await _context.Productos.FindAsync(d.ProductoId);

                    if (producto != null)
                    {
                        // 🔥 REVERSAR STOCK
                        producto.Stock -= d.Cantidad;
                    }
                }

                _context.Compra.Remove(compra);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(ex, "Error al cancelar compra");

                return StatusCode(500, "Error al cancelar compra");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompra(int id)
        {
            var compra = await _context.Compra
                .Include(c => c.Proveedor)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (compra == null)
                return NotFound();

            return Ok(compra);
        }
        [HttpGet("proveedor/{proveedorId}")]
        public async Task<ActionResult<IEnumerable<Compra>>> GetComprasPorProveedor(int proveedorId)
        {
            try
            {
                var compras = await _context.Compra
                    .Include(c => c.Proveedor)
                    .Include(c => c.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Where(c => c.ProveedorId == proveedorId)
                    .OrderByDescending(c => c.Fecha)
                    .ToListAsync();

                return Ok(compras);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener compras del proveedor {ProveedorId}", proveedorId);
                return StatusCode(500, "Error interno del servidor");
            }
        }
        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> DescargarCompraPdf(int id)
        {
            try
            {
                var compra = await _context.Compra
                    .Include(c => c.Proveedor)
                    .Include(c => c.Detalles)
                        .ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (compra == null)
                {
                    return NotFound($"Compra {id} no encontrada");
                }

                var pdfBytes = GenerarPdfCompra(compra);

                var nombreArchivo = $"Compra-{(string.IsNullOrWhiteSpace(compra.SerieFactura) ? "SIN-SERIE" : compra.SerieFactura)}-{(string.IsNullOrWhiteSpace(compra.FolioFactura) ? compra.Id.ToString() : compra.FolioFactura)}.pdf";

                return File(pdfBytes, "application/pdf", nombreArchivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de compra {Id}", id);
                return StatusCode(500, "Error al generar el PDF");
            }
        }

        private byte[] GenerarPdfCompra(Compra compra)
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
                            col.Item().Text("ORDEN DE COMPRA / ENTRADA").FontSize(10).SemiBold();
                            col.Item().Text("Control interno de adquisiciones").FontSize(9);
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Background(colorPrimario).Padding(5).Text("DETALLE DE COMPRA").FontColor(Colors.White).Bold().AlignCenter();

                            col.Item().Border(1).BorderColor(colorPrimario).Padding(5).AlignCenter().Column(innerCol =>
                            {
                                innerCol.Item().Text("FACTURA").FontSize(8).Bold();
                                innerCol.Item().Text($"{compra.SerieFactura ?? "S/F"}-{compra.FolioFactura ?? compra.Id.ToString()}").FontSize(14).Bold().FontColor(colorPrimario);
                            });

                            col.Item().PaddingTop(5).Text($"Fecha: {compra.Fecha:dd/MM/yyyy HH:mm}").FontSize(9);
                        });
                    });

                    page.Content().PaddingVertical(20).Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Border(1).BorderColor(colorBorde).Padding(10).Column(col =>
                            {
                                col.Item().Text("PROVEEDOR").FontSize(8).Bold().FontColor(colorPrimario);
                                col.Item().Text(compra.Proveedor?.Nombre ?? "Sin proveedor").FontSize(11).Bold();
                                col.Item().Text($"RFC: {compra.RFCProveedor ?? compra.Proveedor?.RFC ?? "N/A"}").FontSize(9);
                                col.Item().Text($"Email: {compra.Proveedor?.Email ?? "N/A"}").FontSize(9).FontColor(colorTextoSuave);
                                col.Item().Text($"Teléfono: {compra.Proveedor?.Telefono ?? "N/A"}").FontSize(9).FontColor(colorTextoSuave);
                            });

                            row.ConstantItem(15);

                            row.RelativeItem().Border(1).BorderColor(colorBorde).Padding(10).Column(col =>
                            {
                                col.Item().Text("DATOS FISCALES").FontSize(8).Bold().FontColor(colorPrimario);
                                col.Item().Text($"Serie: {compra.SerieFactura ?? "-"}").FontSize(9);
                                col.Item().Text($"Folio: {compra.FolioFactura ?? "-"}").FontSize(9);
                                col.Item().Text($"Fecha factura: {compra.FechaFactura:dd/MM/yyyy}").FontSize(9);
                                col.Item().Text($"Estado: {compra.Estado}").FontSize(9);
                            });
                        });

                        column.Item().PaddingVertical(15);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4);   // Producto
                                columns.RelativeColumn(1.2f); // Cantidad
                                columns.RelativeColumn(1.7f); // Costo unitario
                                columns.RelativeColumn(1.7f); // Importe
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("PRODUCTO");
                                header.Cell().Element(CellStyle).AlignCenter().Text("CANT.");
                                header.Cell().Element(CellStyle).AlignRight().Text("COSTO UNIT.");
                                header.Cell().Element(CellStyle).AlignRight().Text("IMPORTE");

                                static IContainer CellStyle(IContainer container) =>
                                    container.DefaultTextStyle(x => x.SemiBold().FontColor(Colors.White))
                                             .Background("#3c3c3c")
                                             .PaddingVertical(5)
                                             .PaddingHorizontal(5);
                            });

                            foreach (var detalle in compra.Detalles)
                            {
                                table.Cell().Element(RowStyle).Text(detalle.Producto?.Nombre ?? "Sin nombre");
                                table.Cell().Element(RowStyle).AlignCenter().Text(detalle.Cantidad.ToString());
                                table.Cell().Element(RowStyle).AlignRight().Text($"{detalle.PrecioUnitario:C}");
                                table.Cell().Element(RowStyle).AlignRight().Text($"{detalle.Subtotal:C}");

                                static IContainer RowStyle(IContainer container) =>
                                    container.BorderBottom(1)
                                             .BorderColor(Colors.Grey.Lighten3)
                                             .PaddingVertical(8)
                                             .PaddingHorizontal(5);
                            }
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem();

                            row.ConstantItem(220).PaddingTop(10).Column(col =>
                            {
                                col.Item().Row(r =>
                                {
                                    r.RelativeItem().Text("SUBTOTAL").FontSize(9);
                                    r.RelativeItem().AlignRight().Text($"{compra.Subtotal:C}");
                                });

                                col.Item().Row(r =>
                                {
                                    r.RelativeItem().Text("IVA (16%)").FontSize(9);
                                    r.RelativeItem().AlignRight().Text($"{compra.IVA:C}");
                                });

                                col.Item().PaddingTop(5).Background(colorGrisClaro).Padding(5).Row(r =>
                                {
                                    r.RelativeItem().Text("TOTAL").Bold().FontSize(12).FontColor(colorPrimario);
                                    r.RelativeItem().AlignRight().Text($"{compra.Total:C}").Bold().FontSize(12).FontColor(colorPrimario);
                                });
                            });
                        });

                        column.Item().PaddingTop(30).Text("Documento interno de control de compras.").FontSize(8).FontColor(colorTextoSuave);
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
