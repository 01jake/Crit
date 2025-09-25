using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crit.Client.Services
{
    public class ArticuloService
    {
        private readonly HttpClient _httpClient;

        public ArticuloService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Articulo>> GetArticulosAsync()
        {
            try
            {
                var articulos = await _httpClient.GetFromJsonAsync<List<Articulo>>("api/Articulos");
                return articulos ?? new List<Articulo>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al obtener artículos: {ex.Message}");
                return new List<Articulo>();
            }
        }

        public async Task<Articulo?> GetArticuloByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Articulo>($"api/Articulos/{id}");
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404"))
            {
                return null;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al obtener artículo {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CreateArticuloAsync(Articulo articulo)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Articulos", articulo);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al crear artículo: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateArticuloAsync(Articulo articulo)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/Articulos/{articulo.Id}", articulo);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al actualizar artículo: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteArticuloAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/Articulos/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al eliminar artículo: {ex.Message}");
                return false;
            }
        }
    }
}