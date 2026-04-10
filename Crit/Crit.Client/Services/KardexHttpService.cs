using Crit.Shared.DTOs;
using Microsoft.Extensions.Logging;

namespace Crit.Client.Services
{
    public class KardexHttpService : HttpServiceBase
    {
        public KardexHttpService(HttpClient http, ILogger<KardexHttpService> logger)
            : base(http, logger)
        {
        }

        public Task<List<KardexMovimientoDto>> GetAsync(
            int? productoId = null,
            int? almacenId = null,
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null)
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

            return base.GetListAsync<KardexMovimientoDto>(url);
        }
    }
}
