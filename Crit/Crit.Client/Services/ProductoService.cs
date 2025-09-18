using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;

namespace Crit.Client.Services
{
    public class ProductoService
    {
        private readonly HttpClient _httpClient;

        public ProductoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Producto>> GetProductosAsync()
        {
            try
            {
                var productos = await _httpClient.GetFromJsonAsync<List<Producto>>("api/Productos");
                return productos ?? new List<Producto>();
            }
            catch (HttpRequestException ex)
            {
                // Log del error
                Console.WriteLine($"Error al obtener productos: {ex.Message}");
                throw new ApplicationException("Error al obtener los productos.", ex);
            }
        }

        public async Task<Producto?> GetProductoByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Producto>($"api/Productos/{id}");
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404"))
            {
                return null; // Producto no encontrado
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al obtener producto {id}: {ex.Message}");
                throw new ApplicationException($"Error al obtener el producto {id}.", ex);
            }
        }

        public async Task<bool> CreateProductoAsync(Producto producto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Productos", producto);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al crear producto: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateProductoAsync(Producto producto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/Productos/{producto.Id}", producto);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al actualizar producto: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteProductoAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/Productos/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al eliminar producto: {ex.Message}");
                return false;
            }
        }
    }
}