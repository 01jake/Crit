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

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return new List<Cliente>();

                response.EnsureSuccessStatusCode();

                var clientes = await response.Content.ReadFromJsonAsync<List<Cliente>>();
                return clientes ?? new List<Cliente>();
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
                return await _httpClient.GetFromJsonAsync<Cliente>($"api/clientes/{id}");
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
                var clientes = await _httpClient.GetFromJsonAsync<List<Cliente>>("api/clientes/activos");
                return clientes ?? new List<Cliente>();
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
                var clienteCreado = await response.Content.ReadFromJsonAsync<Cliente>();
                return clienteCreado ?? throw new Exception("Error al crear cliente");
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
                var count = await _httpClient.GetFromJsonAsync<int>("api/clientes/count");
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conteo de clientes");
                throw;
            }
        }
    }
}
