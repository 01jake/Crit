using System.Net.Http.Json;
using Crit.Shared.DTOs;
using Crit.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Crit.Client.Services
{
    public class ReabastecimientoHttpService
    {
        private readonly HttpClient _http;
        private readonly ILogger<ReabastecimientoHttpService> _logger;

        public ReabastecimientoHttpService(HttpClient http, ILogger<ReabastecimientoHttpService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<OrdenReabastecimiento>> GetAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<List<OrdenReabastecimiento>>("api/reabastecimiento")
                    ?? new List<OrdenReabastecimiento>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ordenes de reabastecimiento");
                return new List<OrdenReabastecimiento>();
            }
        }

        public async Task<bool> GenerarAlertasAsync()
        {
            try
            {
                var response = await _http.PostAsync("api/reabastecimiento/generar-alertas", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar alertas de reabastecimiento");
                return false;
            }
        }

        public async Task<bool> CreateAsync(OrdenReabastecimiento orden)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/reabastecimiento", orden);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear orden de reabastecimiento");
                return false;
            }
        }

        public async Task<bool> CambiarEstadoAsync(int id, string accion)
        {
            try
            {
                var response = await _http.PostAsync($"api/reabastecimiento/{id}/{accion}", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de la orden {Id}", id);
                return false;
            }
        }

        public async Task<bool> CrearCompraAsync(int id, CrearCompraDesdeReabastecimientoDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync($"api/reabastecimiento/{id}/crear-compra", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear compra desde orden {Id}", id);
                return false;
            }
        }

        public async Task<bool> CrearTraspasoAsync(int id, CrearTraspasoDesdeReabastecimientoDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync($"api/reabastecimiento/{id}/crear-traspaso", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear traspaso desde orden {Id}", id);
                return false;
            }
        }
        public async Task<bool> VincularCompraAsync(int ordenId, int compraId)
        {
            try
            {
                var response = await _http.PostAsync($"api/reabastecimiento/{ordenId}/vincular-compra/{compraId}", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al vincular compra {CompraId} con orden {OrdenId}", compraId, ordenId);
                return false;
            }
        }

        public async Task<bool> VincularTraspasoAsync(int ordenId, int traspasoId)
        {
            try
            {
                var response = await _http.PostAsync($"api/reabastecimiento/{ordenId}/vincular-traspaso/{traspasoId}", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al vincular traspaso {TraspasoId} con orden {OrdenId}", traspasoId, ordenId);
                return false;
            }
        }
        public async Task<bool> CompletarDesdeCompraAsync(int ordenId, int compraId)
        {
            try
            {
                var response = await _http.PostAsync($"api/reabastecimiento/{ordenId}/completar-desde-compra/{compraId}", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar orden {OrdenId} desde compra {CompraId}", ordenId, compraId);
                return false;
            }
        }

        public async Task<bool> CompletarDesdeTraspasoAsync(int ordenId, int traspasoId)
        {
            try
            {
                var response = await _http.PostAsync($"api/reabastecimiento/{ordenId}/completar-desde-traspaso/{traspasoId}", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar orden {OrdenId} desde traspaso {TraspasoId}", ordenId, traspasoId);
                return false;
            }
        }


    }
}
