using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crit.Client.Services
{
    public class AdminService
    {
        private readonly HttpClient _httpClient;

        public AdminService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Métodos para funcionalidades de administrador
        public async Task<List<string>> GetUsersAsync()
        {
            try
            {
                var users = await _httpClient.GetFromJsonAsync<List<string>>("api/Admin/users");
                return users ?? new List<string>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al obtener usuarios: {ex.Message}");
                return new List<string>();
            }
        }

        // Agregar más métodos según necesites
    }
}