using System.Net;
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
                var response = await _httpClient.GetAsync("api/ventas");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener ventas");
                    return new List<Venta>();
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("403 al obtener ventas");
                    return new List<Venta>();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<Venta>>() ?? new List<Venta>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas");
                return new List<Venta>();
            }
        }

        public async Task<Venta?> GetVentaAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/ventas/{id}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener venta {Id}", id);
                    return null;
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("403 al obtener venta {Id}", id);
                    return null;
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<Venta>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener venta {Id}", id);
                return null;
            }
        }

        public Task<Venta?> GetVentaConDetallesAsync(int id)
            => GetVentaAsync(id);

        public async Task<List<Venta>> GetVentasPorClienteAsync(int clienteId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/ventas/cliente/{clienteId}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener ventas del cliente {ClienteId}", clienteId);
                    return new List<Venta>();
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("403 al obtener ventas del cliente {ClienteId}", clienteId);
                    return new List<Venta>();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<Venta>>() ?? new List<Venta>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas del cliente {ClienteId}", clienteId);
                return new List<Venta>();
            }
        }

        public async Task<List<Venta>> GetVentasPorFechaAsync(DateTime desde, DateTime hasta)
        {
            try
            {
                var desdeStr = desde.ToString("yyyy-MM-dd");
                var hastaStr = hasta.ToString("yyyy-MM-dd");

                var response = await _httpClient.GetAsync(
                    $"api/ventas/fecha?desde={desdeStr}&hasta={hastaStr}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener ventas por fecha");
                    return new List<Venta>();
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("403 al obtener ventas por fecha");
                    return new List<Venta>();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<Venta>>() ?? new List<Venta>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas por fecha");
                return new List<Venta>();
            }
        }

        public async Task<List<Venta>> GetVentasRecientesAsync(int cantidad = 50)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/ventas/recientes?cantidad={cantidad}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener ventas recientes");
                    return new List<Venta>();
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("403 al obtener ventas recientes");
                    return new List<Venta>();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<Venta>>() ?? new List<Venta>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas recientes");
                return new List<Venta>();
            }
        }

        public async Task<Venta?> CreateVentaAsync(Venta venta)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/ventas", venta);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al crear venta");
                    return null;
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("403 al crear venta");
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error HTTP {StatusCode} al crear venta. Respuesta: {Error}", response.StatusCode, error);
                    throw new Exception(string.IsNullOrWhiteSpace(error) ? "No se pudo crear la venta." : error);
                }


                return await response.Content.ReadFromJsonAsync<Venta>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear venta");
                return null;
            }
        }

        public async Task<decimal> GetTotalVentasMesAsync(int mes, int año)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/ventas/total-mes?mes={mes}&año={año}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener total de ventas del mes");
                    return 0m;
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("403 al obtener total de ventas del mes");
                    return 0m;
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<decimal>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener total de ventas del mes");
                return 0m;
            }
        }
    }
}
