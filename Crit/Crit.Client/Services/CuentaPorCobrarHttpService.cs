using System.Net;
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
                var response = await _httpClient.GetAsync("api/CuentasPorCobrar");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener cuentas por cobrar");
                    return new List<CuentaPorCobrar>();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<CuentaPorCobrar>>()
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
                var response = await _httpClient.GetAsync($"api/CuentasPorCobrar/{id}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener cuenta por cobrar {Id}", id);
                    return null;
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<CuentaPorCobrar>();
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
                var response = await _httpClient.GetAsync($"api/CuentasPorCobrar/cliente/{clienteId}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener cuentas por cobrar del cliente {ClienteId}", clienteId);
                    return new List<CuentaPorCobrar>();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<CuentaPorCobrar>>()
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
                var response = await _httpClient.GetAsync("api/CuentasPorCobrar/pendientes");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener cuentas por cobrar pendientes");
                    return new List<CuentaPorCobrar>();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<CuentaPorCobrar>>()
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

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al crear cuenta por cobrar");
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error HTTP {StatusCode} al crear cuenta por cobrar", response.StatusCode);
                    return null;
                }

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

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al registrar pago en cuenta por cobrar {CuentaId}", cuentaId);
                    return false;
                }

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

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al cancelar cuenta por cobrar {CuentaId}", cuentaId);
                    return false;
                }

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
