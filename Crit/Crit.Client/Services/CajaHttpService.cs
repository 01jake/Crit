using System.Net.Http.Json;
using Crit.Shared.DTOs;
using Crit.Shared.Models;

namespace Crit.Client.Services
{
    public class CajaHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CajaHttpService> _logger;

        public CajaHttpService(HttpClient httpClient, ILogger<CajaHttpService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<CajaSesion?> GetCajaActualAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<CajaSesion>("api/caja/actual");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener caja actual");
                return null;
            }
        }

        public async Task<CajaResumenDto> GetResumenAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<CajaResumenDto>("api/caja/resumen")
                       ?? new CajaResumenDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener resumen de caja");
                return new CajaResumenDto();
            }
        }

        public async Task<List<CajaMovimiento>> GetMovimientosAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<CajaMovimiento>>("api/caja/movimientos")
                       ?? new List<CajaMovimiento>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener movimientos de caja");
                return new List<CajaMovimiento>();
            }
        }

        public async Task<bool> AbrirCajaAsync(AperturaCajaDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/caja/abrir", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al abrir caja");
                return false;
            }
        }

        public async Task<bool> CerrarCajaAsync(CierreCajaDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/caja/cerrar", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar caja");
                return false;
            }
        }

        public async Task<List<FlujoCajaRealDto>> GetCashFlowRealAsync(int meses = 6)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<FlujoCajaRealDto>>($"api/caja/cash-flow-real?meses={meses}")
                       ?? new List<FlujoCajaRealDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cash flow real");
                return new List<FlujoCajaRealDto>();
            }
        }
    }
}
