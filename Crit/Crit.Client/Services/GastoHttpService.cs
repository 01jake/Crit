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
                return await _httpClient.GetFromJsonAsync<List<Gasto>>("api/gastos")
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
                return await _httpClient.GetFromJsonAsync<Gasto>($"api/gastos/{id}");
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
                return response.IsSuccessStatusCode;
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
