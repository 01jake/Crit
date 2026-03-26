using System.Net.Http.Json;
using Crit.Shared.Models;

namespace Crit.Client.Services
{
    public class CuentaPorCobrarHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CuentaPorCobrarHttpService> _logger;

        public CuentaPorCobrarHttpService(HttpClient httpClient, ILogger<CuentaPorCobrarHttpService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<CuentaPorCobrar>> GetAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<CuentaPorCobrar>>("api/CuentasPorCobrar")
                    ?? new List<CuentaPorCobrar>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuentas por cobrar");
                return new List<CuentaPorCobrar>();
            }
        }

        public async Task<CuentaPorCobrar?> GetByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<CuentaPorCobrar>($"api/CuentasPorCobrar/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuenta por cobrar {Id}", id);
                return null;
            }
        }

        public async Task<List<CuentaPorCobrar>> GetByClienteAsync(int clienteId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<CuentaPorCobrar>>($"api/CuentasPorCobrar/cliente/{clienteId}")
                    ?? new List<CuentaPorCobrar>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuentas por cobrar del cliente {ClienteId}", clienteId);
                return new List<CuentaPorCobrar>();
            }
        }

        public async Task<List<CuentaPorCobrar>> GetPendientesAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<CuentaPorCobrar>>("api/CuentasPorCobrar/pendientes")
                    ?? new List<CuentaPorCobrar>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuentas por cobrar pendientes");
                return new List<CuentaPorCobrar>();
            }
        }

        public async Task<CuentaPorCobrar?> CrearAsync(CuentaPorCobrar cuenta)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/CuentasPorCobrar", cuenta);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<CuentaPorCobrar>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cuenta por cobrar");
                return null;
            }
        }

        public async Task<bool> RegistrarPagoAsync(int cuentaId, PagoCliente pago)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/CuentasPorCobrar/{cuentaId}/registrar-pago", pago);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar pago en cuenta por cobrar {CuentaId}", cuentaId);
                return false;
            }
        }

        public async Task<bool> CancelarAsync(int cuentaId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/CuentasPorCobrar/{cuentaId}/cancelar", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar cuenta por cobrar {CuentaId}", cuentaId);
                return false;
            }
        }
    }
}
