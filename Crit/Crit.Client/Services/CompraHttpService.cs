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
                var data = await _http.GetFromJsonAsync<List<Compra>>("api/compras");
                return data ?? new List<Compra>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener compras");
                return new List<Compra>();
            }
        }

        public async Task<bool> CrearAsync(Compra compra)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/compras", compra);
                response.EnsureSuccessStatusCode();

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al crear compra: {Error}", error);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear compra");
                return false;
            }
        }

        public async Task<bool> CancelarAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/compras/{id}");
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
            return await _http.GetFromJsonAsync<List<Compra>>("api/compras/historial")
                   ?? new List<Compra>();
        }
        public async Task<Compra?> GetCompraAsync(int id)
        {
            return await _http.GetFromJsonAsync<Compra>($"api/compras/{id}");
        }
        public async Task<List<Compra>> GetByProveedorAsync(int proveedorId)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<Compra>>($"api/compras/proveedor/{proveedorId}")
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