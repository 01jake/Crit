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
                return await _httpClient.GetFromJsonAsync<List<CuentaPorPagar>>("api/CuentasPorPagar")
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
                return await _httpClient.GetFromJsonAsync<CuentaPorPagar>($"api/CuentasPorPagar/{id}");
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
                return await _httpClient.GetFromJsonAsync<List<CuentaPorPagar>>($"api/CuentasPorPagar/proveedor/{proveedorId}")
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
                return await _httpClient.GetFromJsonAsync<List<CuentaPorPagar>>("api/CuentasPorPagar/pendientes")
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
                response.EnsureSuccessStatusCode();
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
