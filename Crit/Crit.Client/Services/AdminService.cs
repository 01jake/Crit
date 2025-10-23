using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Crit.Shared.Models;

namespace Crit.Client.Services
{
    public class AdminService
    {
        private readonly HttpClient _httpClient;

        public AdminService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Obtener todos los usuarios registrados (excepto admins)
        public async Task<List<UsuarioDto>> GetUsuariosAsync()
        {
            try
            {
                var usuarios = await _httpClient.GetFromJsonAsync<List<UsuarioDto>>("api/Quejas/usuarios");
                return usuarios ?? new List<UsuarioDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al obtener usuarios: {ex.Message}");
                return new List<UsuarioDto>();
            }
        }

        // Obtener estadísticas de un usuario específico
        public async Task<UsuarioEstadisticasDto?> GetEstadisticasUsuarioAsync(string usuarioId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<UsuarioEstadisticasDto>($"api/Quejas/usuario/{usuarioId}/estadisticas");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al obtener estadísticas del usuario: {ex.Message}");
                return null;
            }
        }

        // Asignar una queja a un usuario
        public async Task<bool> AsignarQuejaAsync(int quejaId, string usuarioId)
        {
            try
            {
                var dto = new { UsuarioId = usuarioId };
                var response = await _httpClient.PutAsJsonAsync($"api/Quejas/{quejaId}/asignar", dto);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al asignar queja: {ex.Message}");
                return false;
            }
        }

        // Obtener todas las quejas (Admin)
        public async Task<List<Queja>> GetAllQuejasAsync()
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

        // Actualizar estatus de una queja
        public async Task<bool> UpdateQuejaStatusAsync(int quejaId, EstatusQueja nuevoEstatus)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/Quejas/{quejaId}/status", nuevoEstatus);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al actualizar estatus: {ex.Message}");
                return false;
            }
        }

        // Eliminar una queja
        public async Task<bool> DeleteQuejaAsync(int quejaId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/Quejas/{quejaId}");
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