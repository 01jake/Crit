using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;

namespace Crit.Client.Services
{
    public class QuejaService
    {
        private readonly HttpClient _httpClient;

        public QuejaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Para que un cliente envíe una queja
        public async Task<bool> CreateQuejaAsync(Queja queja)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Quejas", queja);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al crear queja: {ex.Message}");
                return false;
            }
        }

        // Para que el administrador liste todas las quejas
        public async Task<List<Queja>> GetQuejasAsync()
        {
            try
            {
                var quejas = await _httpClient.GetFromJsonAsync<List<Queja>>("api/Quejas");
                return quejas ?? new List<Queja>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al obtener quejas: {ex.Message}");
                return new List<Queja>();
            }
        }

        // Para que el usuario vea sus propias quejas
        public async Task<List<Queja>> GetMisQuejasAsync()
        {
            try
            {
                var quejas = await _httpClient.GetFromJsonAsync<List<Queja>>("api/Quejas/mis-quejas");
                return quejas ?? new List<Queja>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al obtener mis quejas: {ex.Message}");
                return new List<Queja>();
            }
        }

        // Para obtener una queja por ID
        public async Task<Queja?> GetQuejaByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Queja>($"api/Quejas/{id}");
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404"))
            {
                return null;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al obtener queja {id}: {ex.Message}");
                return null;
            }
        }

        // Para actualizar el estatus de una queja (Admin)
        public async Task<bool> UpdateQuejaStatusAsync(int id, EstatusQueja nuevoEstatus)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/Quejas/{id}/status", nuevoEstatus);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al actualizar estatus: {ex.Message}");
                return false;
            }
        }

        // Para eliminar una queja (Admin)
        public async Task<bool> DeleteQuejaAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/Quejas/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al eliminar queja: {ex.Message}");
                return false;
            }
        }
    }
}