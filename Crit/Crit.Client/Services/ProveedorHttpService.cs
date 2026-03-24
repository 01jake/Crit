using System.Net.Http;
using System.Net.Http.Json;
using Crit.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Crit.Client.Services
{
    public class ProveedorHttpService
    {
        private readonly HttpClient _http;
        private readonly ILogger<ProveedorHttpService> _logger;

        public ProveedorHttpService(HttpClient http, ILogger<ProveedorHttpService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<Proveedor>> GetAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<List<Proveedor>>("api/proveedores")
                       ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener proveedores");
                return new();
            }
        }

        public async Task<bool> CreateAsync(Proveedor proveedor)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/proveedores", proveedor);
                return res.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear proveedor");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Proveedor proveedor)
        {
            try
            {
                var res = await _http.PutAsJsonAsync($"api/proveedores/{proveedor.Id}", proveedor);
                return res.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar proveedor");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var res = await _http.DeleteAsync($"api/proveedores/{id}");
                return res.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar proveedor");
                return false;
            }
        }
    }
}
