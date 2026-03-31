using System.Net.Http.Json;
using Crit.Shared.DTOs;
using Microsoft.Extensions.Logging;

namespace Crit.Client.Services
{
    public class KardexHttpService
    {
        private readonly HttpClient _http;
        private readonly ILogger<KardexHttpService> _logger;

        public KardexHttpService(HttpClient http, ILogger<KardexHttpService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<KardexMovimientoDto>> GetAsync(
            int? productoId = null,
            int? almacenId = null,
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null)
        {
            try
            {
                var query = new List<string>();

                if (productoId.HasValue)
                    query.Add($"productoId={productoId.Value}");

                if (almacenId.HasValue)
                    query.Add($"almacenId={almacenId.Value}");

                if (fechaInicio.HasValue)
                    query.Add($"fechaInicio={fechaInicio.Value:yyyy-MM-dd}");

                if (fechaFin.HasValue)
                    query.Add($"fechaFin={fechaFin.Value:yyyy-MM-dd}");

                var url = "api/kardex";
                if (query.Any())
                    url += "?" + string.Join("&", query);

                return await _http.GetFromJsonAsync<List<KardexMovimientoDto>>(url)
                    ?? new List<KardexMovimientoDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener kardex");
                return new List<KardexMovimientoDto>();
            }
        }
    }
}
