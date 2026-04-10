using System.Net;
using System.Net.Http.Json;
using Crit.Shared.DTOs;
using Crit.Shared.Models;

namespace Crit.Client.Services
{
    public class CotizacionHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CotizacionHttpService> _logger;

        public CotizacionHttpService(HttpClient httpClient, ILogger<CotizacionHttpService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<Cotizacion>> GetCotizacionesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/cotizaciones");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener cotizaciones");
                    return new List<Cotizacion>();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<Cotizacion>>() ?? new List<Cotizacion>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cotizaciones");
                return new List<Cotizacion>();
            }
        }

        public async Task<Cotizacion?> GetCotizacionAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/cotizaciones/{id}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener cotización {Id}", id);
                    return null;
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<Cotizacion>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cotización {Id}", id);
                return null;
            }
        }

        public async Task<List<Cotizacion>> GetCotizacionesPorClienteAsync(int clienteId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/cotizaciones/cliente/{clienteId}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener cotizaciones del cliente {ClienteId}", clienteId);
                    return new List<Cotizacion>();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<Cotizacion>>() ?? new List<Cotizacion>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cotizaciones del cliente {ClienteId}", clienteId);
                return new List<Cotizacion>();
            }
        }

        public async Task<Cotizacion?> CreateCotizacionAsync(Cotizacion cotizacion)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/cotizaciones", cotizacion);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al crear cotización");
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error HTTP {StatusCode} al crear cotización", response.StatusCode);
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<Cotizacion>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cotización");
                return null;
            }
        }

        public async Task<bool> UpdateCotizacionAsync(Cotizacion cotizacion)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/cotizaciones/{cotizacion.Id}", cotizacion);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al actualizar cotización {Id}", cotizacion.Id);
                    return false;
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cotización {Id}", cotizacion.Id);
                return false;
            }
        }

        public async Task<bool> DeleteCotizacionAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/cotizaciones/{id}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al eliminar cotización {Id}", id);
                    return false;
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cotización {Id}", id);
                return false;
            }
        }

        public async Task<Venta?> ConvertirAVentaAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/cotizaciones/{id}/convertir-venta", null);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al convertir cotización {Id} a venta", id);
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error HTTP {StatusCode} al convertir cotización {Id}", response.StatusCode, id);
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<Venta>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al convertir cotización {Id} a venta", id);
                return null;
            }
        }

        public async Task<byte[]?> GenerarPdfAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/cotizaciones/{id}/pdf");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al generar PDF de cotización {Id}", id);
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error HTTP {StatusCode} al generar PDF de cotización {Id}", response.StatusCode, id);
                    return null;
                }

                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de cotización {Id}", id);
                return null;
            }
        }
    }
}
