using System.Net.Http.Json;
using Crit.Shared.Models;

namespace Crit.Client.Services
{
    public class ProductoHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductoHttpService> _logger;

        public ProductoHttpService(HttpClient httpClient, ILogger<ProductoHttpService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<Producto>> GetProductosAsync()
        {
            try
            {
                var productos = await _httpClient.GetFromJsonAsync<List<Producto>>("api/productos");
                return productos ?? new List<Producto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos");
                throw;
            }
        }

        public async Task<Producto?> GetProductoAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Producto>($"api/productos/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener producto {Id}", id);
                throw;
            }
        }

        public async Task<List<Producto>> GetProductosActivosAsync()
        {
            try
            {
                var productos = await _httpClient.GetFromJsonAsync<List<Producto>>("api/productos/activos");
                return productos ?? new List<Producto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos activos");
                throw;
            }
        }

        public async Task<List<Producto>> GetProductosBajoStockAsync()
        {
            try
            {
                var productos = await _httpClient.GetFromJsonAsync<List<Producto>>("api/productos/bajo-stock");
                return productos ?? new List<Producto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos con bajo stock");
                throw;
            }
        }

        public async Task<Producto> CreateProductoAsync(Producto producto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/productos", producto);
                response.EnsureSuccessStatusCode();
                var productoCreado = await response.Content.ReadFromJsonAsync<Producto>();
                return productoCreado ?? throw new Exception("Error al crear producto");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear producto");
                throw;
            }
        }

        public async Task UpdateProductoAsync(Producto producto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/productos/{producto.Id}", producto);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar producto {Id}", producto.Id);
                throw;
            }
        }

        public async Task DeleteProductoAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/productos/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar producto {Id}", id);
                throw;
            }
        }

        public async Task ActualizarStockAsync(int id, int cantidad)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/productos/{id}/stock", cantidad);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar stock del producto {Id}", id);
                throw;
            }
        }
        public async Task<int> GetProductosCountAsync()
        {
            try
            {
                var count = await _httpClient.GetFromJsonAsync<int>("api/productos/count");
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conteo de productos");
                throw;
            }
        }
    }
}
