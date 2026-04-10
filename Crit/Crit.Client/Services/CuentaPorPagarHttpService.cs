using System.Net;
using System.Net.Http.Json;
using Crit.Shared.Models;

namespace Crit.Client.Services
{
    public class CuentaPorPagarHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CuentaPorPagarHttpService> _logger;

        public CuentaPorPagarHttpService(HttpClient httpClient, ILogger<CuentaPorPagarHttpService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<CuentaPorPagar>> GetAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/CuentasPorPagar");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener cuentas por pagar");
                    return new List<CuentaPorPagar>();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<CuentaPorPagar>>()
                    ?? new List<CuentaPorPagar>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuentas por pagar");
                return new List<CuentaPorPagar>();
            }
        }

        public async Task<CuentaPorPagar?> GetByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/CuentasPorPagar/{id}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener cuenta por pagar {Id}", id);
                    return null;
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<CuentaPorPagar>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuenta por pagar {Id}", id);
                return null;
            }
        }

        public async Task<List<CuentaPorPagar>> GetByProveedorAsync(int proveedorId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/CuentasPorPagar/proveedor/{proveedorId}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener cuentas por pagar del proveedor {ProveedorId}", proveedorId);
                    return new List<CuentaPorPagar>();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<CuentaPorPagar>>()
                    ?? new List<CuentaPorPagar>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuentas por pagar del proveedor {ProveedorId}", proveedorId);
                return new List<CuentaPorPagar>();
            }
        }

        public async Task<List<CuentaPorPagar>> GetPendientesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/CuentasPorPagar/pendientes");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener cuentas por pagar pendientes");
                    return new List<CuentaPorPagar>();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<CuentaPorPagar>>()
                    ?? new List<CuentaPorPagar>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuentas por pagar pendientes");
                return new List<CuentaPorPagar>();
            }
        }

        public async Task<CuentaPorPagar?> CrearAsync(CuentaPorPagar cuenta)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/CuentasPorPagar", cuenta);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al crear cuenta por pagar");
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error HTTP {StatusCode} al crear cuenta por pagar", response.StatusCode);
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<CuentaPorPagar>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cuenta por pagar");
                return null;
            }
        }

        public async Task<bool> RegistrarPagoAsync(int cuentaId, PagoProveedor pago)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/CuentasPorPagar/{cuentaId}/registrar-pago", pago);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al registrar pago en cuenta por pagar {CuentaId}", cuentaId);
                    return false;
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar pago en cuenta por pagar {CuentaId}", cuentaId);
                return false;
            }
        }

        public async Task<bool> CancelarAsync(int cuentaId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/CuentasPorPagar/{cuentaId}/cancelar", null);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al cancelar cuenta por pagar {CuentaId}", cuentaId);
                    return false;
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar cuenta por pagar {CuentaId}", cuentaId);
                return false;
            }
        }
    }
}
