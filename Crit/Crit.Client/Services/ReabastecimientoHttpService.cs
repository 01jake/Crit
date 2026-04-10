using Crit.Shared.DTOs;
using Crit.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Crit.Client.Services
{
    public class ReabastecimientoHttpService : HttpServiceBase
    {
        public ReabastecimientoHttpService(HttpClient http, ILogger<ReabastecimientoHttpService> logger)
            : base(http, logger)
        {
        }

        public Task<List<OrdenReabastecimiento>> GetAsync()
            => base.GetListAsync<OrdenReabastecimiento>("api/reabastecimiento");

        public Task<bool> GenerarAlertasAsync()
            => base.PostAsync<object?>("api/reabastecimiento/generar-alertas", null);

        public Task<bool> CreateAsync(OrdenReabastecimiento orden)
            => base.PostAsync("api/reabastecimiento", orden);

        public Task<bool> CambiarEstadoAsync(int id, string accion)
            => base.PostAsync<object?>($"api/reabastecimiento/{id}/{accion}", null);

        public Task<bool> CrearCompraAsync(int id, CrearCompraDesdeReabastecimientoDto dto)
            => base.PostAsync($"api/reabastecimiento/{id}/crear-compra", dto);

        public Task<bool> CrearTraspasoAsync(int id, CrearTraspasoDesdeReabastecimientoDto dto)
            => base.PostAsync($"api/reabastecimiento/{id}/crear-traspaso", dto);

        public Task<bool> VincularCompraAsync(int ordenId, int compraId)
            => base.PostAsync<object?>($"api/reabastecimiento/{ordenId}/vincular-compra/{compraId}", null);

        public Task<bool> VincularTraspasoAsync(int ordenId, int traspasoId)
            => base.PostAsync<object?>($"api/reabastecimiento/{ordenId}/vincular-traspaso/{traspasoId}", null);

        public Task<bool> CompletarDesdeCompraAsync(int ordenId, int compraId)
            => base.PostAsync<object?>($"api/reabastecimiento/{ordenId}/completar-desde-compra/{compraId}", null);

        public Task<bool> CompletarDesdeTraspasoAsync(int ordenId, int traspasoId)
            => base.PostAsync<object?>($"api/reabastecimiento/{ordenId}/completar-desde-traspaso/{traspasoId}", null);
    }
}
