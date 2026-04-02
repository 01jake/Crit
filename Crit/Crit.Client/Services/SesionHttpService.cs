using System.Net.Http.Json;
using Crit.Shared.DTOs;
using Microsoft.Extensions.Logging;

namespace Crit.Client.Services
{
    public class SesionHttpService
    {
        private readonly HttpClient _http;
        private readonly ILogger<SesionHttpService> _logger;

        public SesionHttpService(HttpClient http, ILogger<SesionHttpService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<EmpresaSesionDto?> GetEmpresaActualAsync()
        {
            try
            {
                var response = await _http.GetAsync("api/sesion/empresa-actual");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<EmpresaSesionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empresa actual");
                return null;
            }
        }

    }
}
