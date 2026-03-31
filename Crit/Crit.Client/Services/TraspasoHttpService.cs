using System.Net.Http.Json;
using Crit.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Crit.Client.Services
{
    public class TraspasoHttpService
    {
        private readonly HttpClient _http;
        private readonly ILogger<TraspasoHttpService> _logger;

        public TraspasoHttpService(HttpClient http, ILogger<TraspasoHttpService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<TraspasoAlmacen>> GetAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<List<TraspasoAlmacen>>("api/traspasos")
                       ?? new List<TraspasoAlmacen>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener traspasos");
                return new List<TraspasoAlmacen>();
            }
        }

        public async Task<TraspasoAlmacen?> GetByIdAsync(int id)
        {
            try
            {
                return await _http.GetFromJsonAsync<TraspasoAlmacen>($"api/traspasos/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener traspaso {Id}", id);
                return null;
            }
        }

        public async Task<TraspasoAlmacen?> CreateAsync(TraspasoAlmacen traspaso)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/traspasos", traspaso);

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<TraspasoAlmacen>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear traspaso");
                return null;
            }
        }


        public async Task<bool> CancelAsync(int id)
        {
            try
            {
                var response = await _http.PostAsync($"api/traspasos/{id}/cancelar", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar traspaso {Id}", id);
                return false;
            }
        }
    }
}
