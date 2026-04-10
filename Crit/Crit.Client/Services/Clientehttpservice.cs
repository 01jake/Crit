using System.Net;
using System.Net.Http.Json;
using Crit.Shared.Models;

namespace Crit.Client.Services
{
    public class ClienteHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ClienteHttpService> _logger;

        public ClienteHttpService(HttpClient httpClient, ILogger<ClienteHttpService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<Cliente>> GetClientesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/clientes");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener clientes");
                    return new List<Cliente>();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<Cliente>>() ?? new List<Cliente>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes");
                return new List<Cliente>();
            }
        }

        public async Task<Cliente?> GetClienteAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/clientes/{id}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener cliente {Id}", id);
                    return null;
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<Cliente>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cliente {Id}", id);
                throw;
            }
        }

        public async Task<List<Cliente>> GetClientesActivosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/clientes/activos");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener clientes activos");
                    return new List<Cliente>();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<Cliente>>() ?? new List<Cliente>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes activos");
                throw;
            }
        }

        public async Task<Cliente> CreateClienteAsync(Cliente cliente)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/clientes", cliente);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<Cliente>()
                    ?? throw new Exception("Error al crear cliente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cliente");
                throw;
            }
        }

        public async Task UpdateClienteAsync(Cliente cliente)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/clientes/{cliente.Id}", cliente);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cliente {Id}", cliente.Id);
                throw;
            }
        }

        public async Task DeleteClienteAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/clientes/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cliente {Id}", id);
                throw;
            }
        }

        public async Task<int> GetClientesCountAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/clientes/count");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener conteo de clientes");
                    return 0;
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<int>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conteo de clientes");
                throw;
            }
        }
    }
}
