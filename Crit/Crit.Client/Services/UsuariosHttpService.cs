using System.Net;
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
                var response = await _httpClient.GetAsync("api/usuarios");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener usuarios");
                    return new List<UsuarioEmpresaDto>();
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("403 al obtener usuarios");
                    return new List<UsuarioEmpresaDto>();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<UsuarioEmpresaDto>>()
                    ?? new List<UsuarioEmpresaDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                return new List<UsuarioEmpresaDto>();
            }
        }

        public async Task<UsuarioEmpresaDto?> GetUsuarioAsync(string id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/usuarios/{id}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener usuario {Id}", id);
                    return null;
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("403 al obtener usuario {Id}", id);
                    return null;
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<UsuarioEmpresaDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario {Id}", id);
                return null;
            }
        }

        public async Task<bool> CrearUsuarioAsync(CrearUsuarioDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/usuarios", dto);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al crear usuario");
                    return false;
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("403 al crear usuario");
                    return false;
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                return false;
            }
        }

        public async Task<bool> CambiarRolAsync(string id, CambiarRolUsuarioDto dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/usuarios/{id}/rol", dto);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al cambiar rol del usuario {Id}", id);
                    return false;
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("403 al cambiar rol del usuario {Id}", id);
                    return false;
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar rol del usuario {Id}", id);
                return false;
            }
        }

        public async Task<bool> DesactivarUsuarioAsync(string id)
        {
            try
            {
                var response = await _httpClient.PutAsync($"api/usuarios/{id}/desactivar", null);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al desactivar usuario {Id}", id);
                    return false;
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("403 al desactivar usuario {Id}", id);
                    return false;
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar usuario {Id}", id);
                return false;
            }
        }

        public async Task<bool> ActivarUsuarioAsync(string id)
        {
            try
            {
                var response = await _httpClient.PutAsync($"api/usuarios/{id}/activar", null);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al activar usuario {Id}", id);
                    return false;
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("403 al activar usuario {Id}", id);
                    return false;
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al activar usuario {Id}", id);
                return false;
            }
        }
    }
}
