using System.Net;
using System.Net.Http.Json;
using Crit.Shared.DTOs;
using Crit.Shared.Models;

namespace Crit.Client.Services
{
    public class GastoHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GastoHttpService> _logger;

        public GastoHttpService(HttpClient httpClient, ILogger<GastoHttpService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<Gasto>> GetGastosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/gastos");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener gastos");
                    return new List<Gasto>();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<Gasto>>()
                       ?? new List<Gasto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener gastos");
                return new List<Gasto>();
            }
        }

        public async Task<Gasto?> GetGastoAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/gastos/{id}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener gasto {Id}", id);
                    return null;
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<Gasto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener gasto {Id}", id);
                return null;
            }
        }

        public async Task<bool> CrearGastoAsync(RegistrarGastoDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/gastos", dto);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al crear gasto");
                    return false;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error HTTP {StatusCode} al crear gasto. Respuesta: {Error}", response.StatusCode, error);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear gasto");
                return false;
            }
        }

        public async Task<bool> CancelarGastoAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/gastos/{id}/cancelar", null);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al cancelar gasto {Id}", id);
                    return false;
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar gasto {Id}", id);
                return false;
            }
        }
    }
}
