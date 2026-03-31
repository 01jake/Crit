using System.Net.Http.Json;
using Crit.Shared.DTOs;
using Crit.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Crit.Client.Services
{
    public class InventarioAlmacenHttpService
    {
        private readonly HttpClient _http;
        private readonly ILogger<InventarioAlmacenHttpService> _logger;

        public InventarioAlmacenHttpService(HttpClient http, ILogger<InventarioAlmacenHttpService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<InventarioPorAlmacen>> GetAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<List<InventarioPorAlmacen>>("api/inventarioalmacen")
                    ?? new List<InventarioPorAlmacen>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inventario por almacen");
                return new List<InventarioPorAlmacen>();
            }
        }

        public async Task<List<InventarioPorAlmacen>> GetPorAlmacenAsync(int almacenId)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<InventarioPorAlmacen>>($"api/inventarioalmacen/almacen/{almacenId}")
                    ?? new List<InventarioPorAlmacen>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inventario del almacen {AlmacenId}", almacenId);
                return new List<InventarioPorAlmacen>();
            }
        }

        public async Task<List<InventarioPorAlmacen>> GetPorProductoAsync(int productoId)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<InventarioPorAlmacen>>($"api/inventarioalmacen/producto/{productoId}")
                    ?? new List<InventarioPorAlmacen>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inventario del producto {ProductoId}", productoId);
                return new List<InventarioPorAlmacen>();
            }
        }

        public async Task<List<InventarioPorAlmacen>> GetAlertasAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<List<InventarioPorAlmacen>>("api/inventarioalmacen/alertas-minimo")
                    ?? new List<InventarioPorAlmacen>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener alertas de inventario");
                return new List<InventarioPorAlmacen>();
            }
        }

        public async Task<bool> CreateAsync(InventarioPorAlmacen item)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/inventarioalmacen", item);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear inventario por almacen");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(InventarioPorAlmacen item)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"api/inventarioalmacen/{item.Id}", item);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar inventario por almacen {Id}", item.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/inventarioalmacen/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar inventario por almacen {Id}", id);
                return false;
            }
        }
    }
}
