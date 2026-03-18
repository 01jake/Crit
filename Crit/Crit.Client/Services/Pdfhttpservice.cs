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

        public async Task DescargarCotizacionPdfAsync(int cotizacionId)
        {
            try
            {
                // Obtener el PDF como bytes desde el servidor
                var pdfBytes = await _httpClient.GetByteArrayAsync($"api/cotizaciones/{cotizacionId}/pdf");

                // Convertir a Base64
                var base64 = Convert.ToBase64String(pdfBytes);

                // Descargar usando JavaScript
                await _jsRuntime.InvokeVoidAsync(
                    "downloadPdf",
                    $"Cotizacion-{cotizacionId}.pdf",
                    base64
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar PDF de cotización {Id}", cotizacionId);
                throw;
            }
        }

        public async Task DescargarVentaPdfAsync(int ventaId)
        {
            try
            {
                // ✅ CORRECTO: Usa api/ventas/{id}/pdf
                var pdfBytes = await _httpClient.GetByteArrayAsync($"api/ventas/{ventaId}/pdf");
                var base64 = Convert.ToBase64String(pdfBytes);

                await _jsRuntime.InvokeVoidAsync(
                    "downloadPdf",
                    $"Venta-{ventaId}.pdf",
                    base64
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar PDF de venta {Id}", ventaId);
                throw;
            }
        }
        public async Task GenerarVentaPdfAsync(int ventaId)
        {
            try
            {
                // Llama a un endpoint que genera el PDF
                var response = await _httpClient.PostAsync($"api/ventas/{ventaId}/generar-pdf", null);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error al generar PDF: {response.StatusCode}");
                }

                // Leer bytes del PDF
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();

                // Convertir a Base64
                var base64 = Convert.ToBase64String(pdfBytes);

                // Descargar usando JS
                await _jsRuntime.InvokeVoidAsync(
                    "downloadPdf",
                    $"Venta-{ventaId}.pdf",
                    base64
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de venta {Id}", ventaId);
                throw;
            }
        }
    }
}
