using System.Net.Http.Json;
using Crit.Shared.Models;

namespace Crit.Client.Services
{


    public class VentaHttpService 
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<VentaHttpService> _logger;

        public VentaHttpService(HttpClient httpClient, ILogger<VentaHttpService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<Venta>> GetVentasAsync()
        {
            try
            {
                var ventas = await _httpClient.GetFromJsonAsync<List<Venta>>("api/ventas");
                return ventas ?? new List<Venta>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas");
                throw;
            }
        }

        public async Task<Venta?> GetVentaAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Venta>($"api/ventas/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener venta {Id}", id);
                throw;
            }
        }

        public async Task<Venta?> GetVentaConDetallesAsync(int id)
        {
            try
            {
                // Llama al mismo endpoint que GetVentaAsync, 
                // el controller ya retorna con detalles incluidos
                return await _httpClient.GetFromJsonAsync<Venta>($"api/ventas/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener venta con detalles {Id}", id);
                throw;
            }
        }

        public async Task<List<Venta>> GetVentasPorClienteAsync(int clienteId)
        {
            try
            {
                var ventas = await _httpClient.GetFromJsonAsync<List<Venta>>($"api/ventas/cliente/{clienteId}");
                return ventas ?? new List<Venta>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas del cliente {ClienteId}", clienteId);
                throw;
            }
        }

        public async Task<List<Venta>> GetVentasPorFechaAsync(DateTime desde, DateTime hasta)
        {
            try
            {
                // ✅ Formatear las fechas correctamente
                var desdeStr = desde.ToString("yyyy-MM-dd");
                var hastaStr = hasta.ToString("yyyy-MM-dd");

                var ventas = await _httpClient.GetFromJsonAsync<List<Venta>>(
                    $"api/ventas/fecha?desde={desdeStr}&hasta={hastaStr}");

                return ventas ?? new List<Venta>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas por fecha");
                throw;
            }
        }

        public async Task<List<Venta>> GetVentasRecientesAsync(int cantidad = 10)
        {
            try
            {
                var ventas = await _httpClient.GetFromJsonAsync<List<Venta>>(
                    $"api/ventas/recientes?cantidad={cantidad}");
                return ventas ?? new List<Venta>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas recientes");
                throw;
            }
        }

        public async Task<Venta> CreateVentaAsync(Venta venta)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/ventas", venta);
                response.EnsureSuccessStatusCode();
                var ventaCreada = await response.Content.ReadFromJsonAsync<Venta>();
                return ventaCreada ?? throw new Exception("Error al crear venta");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear venta");
                throw;
            }
        }

        public async Task<decimal> GetTotalVentasMesAsync(int mes, int año)
        {
            try
            {
                var total = await _httpClient.GetFromJsonAsync<decimal>(
                    $"api/ventas/total-mes?mes={mes}&año={año}");
                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener total de ventas del mes");
                throw;
            }
        }
    }
}
