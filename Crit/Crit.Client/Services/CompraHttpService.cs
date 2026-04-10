using System.Net;
using System.Net.Http.Json;
using Crit.Shared.Models;

namespace Crit.Client.Services
{
    public class CompraHttpService
    {
        private readonly HttpClient _http;
        private readonly ILogger<CompraHttpService> _logger;

        public CompraHttpService(HttpClient http, ILogger<CompraHttpService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<Compra>> GetAsync()
        {
            try
            {
                var response = await _http.GetAsync("api/compras");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener compras");
                    return new List<Compra>();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<Compra>>()
                       ?? new List<Compra>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener compras");
                return new List<Compra>();
            }
        }

        public async Task<Compra?> CrearAsync(Compra compra)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/compras", compra);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al crear compra");
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error HTTP {StatusCode} al crear compra", response.StatusCode);
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<Compra>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear compra");
                return null;
            }
        }

        public async Task<bool> CancelarAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/compras/{id}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al cancelar compra {Id}", id);
                    return false;
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar compra {Id}", id);
                return false;
            }
        }

        public async Task<List<Compra>> GetHistorialAsync()
        {
            try
            {
                var response = await _http.GetAsync("api/compras/historial");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener historial de compras");
                    return new List<Compra>();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<Compra>>()
                       ?? new List<Compra>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de compras");
                return new List<Compra>();
            }
        }

        public async Task<Compra?> GetCompraAsync(int id)
        {
            try
            {
                var response = await _http.GetAsync($"api/compras/{id}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener compra {Id}", id);
                    return null;
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<Compra>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener compra {Id}", id);
                return null;
            }
        }

        public async Task<List<Compra>> GetByProveedorAsync(int proveedorId)
        {
            try
            {
                var response = await _http.GetAsync($"api/compras/proveedor/{proveedorId}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener compras del proveedor {ProveedorId}", proveedorId);
                    return new List<Compra>();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<Compra>>()
                       ?? new List<Compra>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener compras del proveedor {ProveedorId}", proveedorId);
                return new List<Compra>();
            }
        }
    }
}
