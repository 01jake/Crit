using System.Net;
using Microsoft.JSInterop;

namespace Crit.Client.Services
{
    public class PdfHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<PdfHttpService> _logger;

        public PdfHttpService(
            HttpClient httpClient,
            IJSRuntime jsRuntime,
            ILogger<PdfHttpService> logger)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
            _logger = logger;
        }

        public async Task<bool> DescargarCotizacionPdfAsync(int cotizacionId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/cotizaciones/{cotizacionId}/pdf");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al descargar PDF de cotización {Id}", cotizacionId);
                    return false;
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("403 al descargar PDF de cotización {Id}", cotizacionId);
                    return false;
                }

                response.EnsureSuccessStatusCode();

                var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                var base64 = Convert.ToBase64String(pdfBytes);

                await _jsRuntime.InvokeVoidAsync(
                    "downloadPdf",
                    $"Cotizacion-{cotizacionId}.pdf",
                    base64);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar PDF de cotización {Id}", cotizacionId);
                return false;
            }
        }

        public async Task<bool> DescargarVentaPdfAsync(int ventaId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/ventas/{ventaId}/pdf");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al descargar PDF de venta {Id}", ventaId);
                    return false;
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("403 al descargar PDF de venta {Id}", ventaId);
                    return false;
                }

                response.EnsureSuccessStatusCode();

                var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                var base64 = Convert.ToBase64String(pdfBytes);

                await _jsRuntime.InvokeVoidAsync(
                    "downloadPdf",
                    $"Venta-{ventaId}.pdf",
                    base64);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar PDF de venta {Id}", ventaId);
                return false;
            }
        }

        public async Task<bool> DescargarCompraPdfAsync(int compraId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/compras/{compraId}/pdf");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al descargar PDF de compra {Id}", compraId);
                    return false;
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("403 al descargar PDF de compra {Id}", compraId);
                    return false;
                }

                response.EnsureSuccessStatusCode();

                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                var base64 = Convert.ToBase64String(fileBytes);

                await _jsRuntime.InvokeVoidAsync("downloadFileFromBytes", $"Compra-{compraId}.pdf", base64);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar PDF de compra {Id}", compraId);
                return false;
            }
        }

        public async Task<bool> GenerarVentaPdfAsync(int ventaId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/ventas/{ventaId}/generar-pdf", null);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al generar PDF de venta {Id}", ventaId);
                    return false;
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("403 al generar PDF de venta {Id}", ventaId);
                    return false;
                }

                response.EnsureSuccessStatusCode();

                var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                var base64 = Convert.ToBase64String(pdfBytes);

                await _jsRuntime.InvokeVoidAsync(
                    "downloadPdf",
                    $"Venta-{ventaId}.pdf",
                    base64);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de venta {Id}", ventaId);
                return false;
            }
        }
    }
}
