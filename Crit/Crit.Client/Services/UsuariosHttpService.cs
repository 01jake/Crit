using System.Net.Http.Json;
using Crit.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Crit.Client.Services
{
    public class UsuariosHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UsuariosHttpService> _logger;

        public UsuariosHttpService(HttpClient httpClient, ILogger<UsuariosHttpService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<UsuarioEmpresaDto>> GetUsuariosAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<UsuarioEmpresaDto>>("api/usuarios")
                    ?? new List<UsuarioEmpresaDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                throw;
            }
        }

        public async Task<UsuarioEmpresaDto?> GetUsuarioAsync(string id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<UsuarioEmpresaDto>($"api/usuarios/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario {Id}", id);
                throw;
            }
        }

        public async Task CrearUsuarioAsync(CrearUsuarioDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/usuarios", dto);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                throw;
            }
        }

        public async Task CambiarRolAsync(string id, CambiarRolUsuarioDto dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/usuarios/{id}/rol", dto);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar rol del usuario {Id}", id);
                throw;
            }
        }

        public async Task DesactivarUsuarioAsync(string id)
        {
            try
            {
                var response = await _httpClient.PutAsync($"api/usuarios/{id}/desactivar", null);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar usuario {Id}", id);
                throw;
            }
        }

        public async Task ActivarUsuarioAsync(string id)
        {
            try
            {
                var response = await _httpClient.PutAsync($"api/usuarios/{id}/activar", null);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al activar usuario {Id}", id);
                throw;
            }
        }
    }
}
