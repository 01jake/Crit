using System.Net.Http.Json;
using Crit.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Crit.Client.Services
{
    public class AlmacenHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AlmacenHttpService> _logger;

        public AlmacenHttpService(IHttpClientFactory httpClientFactory, ILogger<AlmacenHttpService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("CritAPI");
            _logger = logger;
        }

        public async Task<List<Almacen>> GetAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Almacen>>("api/almacenes")
                       ?? new List<Almacen>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener almacenes");
                return new List<Almacen>();
            }
        }

        public async Task<List<Almacen>> GetActivosAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Almacen>>("api/almacenes/activos")
                       ?? new List<Almacen>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener almacenes activos");
                return new List<Almacen>();
            }
        }

        public async Task<Almacen?> CreateAsync(Almacen almacen)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/almacenes", almacen);

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<Almacen>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear almacén");
                return null;
            }
        }
    }
}
