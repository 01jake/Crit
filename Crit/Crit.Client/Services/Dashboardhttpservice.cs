using System.Net;
using System.Net.Http.Json;
using Crit.Shared.DTOs;
using Crit.Shared.Models;

namespace Crit.Client.Services
{
    public class Dashboardhttpservice
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<Dashboardhttpservice> _logger;

        public Dashboardhttpservice(HttpClient httpClient, ILogger<Dashboardhttpservice> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<DashboardStatsDto> GetStatsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/dashboard/stats");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener estadísticas del dashboard");
                    return new DashboardStatsDto();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<DashboardStatsDto>() ?? new DashboardStatsDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas del dashboard");
                return new DashboardStatsDto();
            }
        }

        public async Task<List<CashFlowDto>> GetCashFlowAsync(int meses = 6)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/dashboard/cash-flow?meses={meses}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener cash flow");
                    return new List<CashFlowDto>();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<CashFlowDto>>() ?? new List<CashFlowDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cash flow");
                return new List<CashFlowDto>();
            }
        }

        public async Task<List<VentasPorDiaDto>> GetVentasPorDiaAsync(int dias = 30)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/dashboard/ventas-por-dia?dias={dias}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener ventas por día");
                    return new List<VentasPorDiaDto>();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<VentasPorDiaDto>>() ?? new List<VentasPorDiaDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas por día");
                return new List<VentasPorDiaDto>();
            }
        }

        public async Task<List<VentasPorMesDto>> GetVentasPorMesAsync(int meses = 6)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/dashboard/ventas-por-mes?meses={meses}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener ventas por mes");
                    return new List<VentasPorMesDto>();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<VentasPorMesDto>>() ?? new List<VentasPorMesDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas por mes");
                return new List<VentasPorMesDto>();
            }
        }

        public async Task<List<ProductoMasVendidoDto>> GetProductosMasVendidosAsync(int cantidad = 5)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/dashboard/productos-mas-vendidos?cantidad={cantidad}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener productos más vendidos");
                    return new List<ProductoMasVendidoDto>();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<ProductoMasVendidoDto>>() ?? new List<ProductoMasVendidoDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos más vendidos");
                return new List<ProductoMasVendidoDto>();
            }
        }

        public async Task<DashboardAlertaDto> GetAlertasAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/dashboard/alertas");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener alertas");
                    return new DashboardAlertaDto();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<DashboardAlertaDto>() ?? new DashboardAlertaDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener alertas");
                return new DashboardAlertaDto();
            }
        }

        public async Task<List<VentaRecienteDto>> GetVentasRecientesAsync(int cantidad = 5)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/dashboard/ventas-recientes?cantidad={cantidad}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener ventas recientes");
                    return new List<VentaRecienteDto>();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<VentaRecienteDto>>() ?? new List<VentaRecienteDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas recientes");
                return new List<VentaRecienteDto>();
            }
        }

        public async Task<DashboardStatsDto> GetStatsAsync(DateTime? fechaInicio = null)
        {
            try
            {
                var url = fechaInicio.HasValue
                    ? $"api/dashboard/stats?fechaInicio={fechaInicio.Value:yyyy-MM-dd}"
                    : "api/dashboard/stats";

                var response = await _httpClient.GetAsync(url);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener estadísticas filtradas del dashboard");
                    return new DashboardStatsDto();
                }

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<DashboardStatsDto>() ?? new DashboardStatsDto();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error del servidor en dashboard stats: {ErrorContent}", errorContent);
                return new DashboardStatsDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de conexión en GetStatsAsync");
                return new DashboardStatsDto();
            }
        }

        public async Task<FinanzasResumenDto> GetFinanzasResumenAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/dashboard/finanzas-resumen");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("401 al obtener resumen financiero");
                    return new FinanzasResumenDto();
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<FinanzasResumenDto>() ?? new FinanzasResumenDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener resumen financiero");
                return new FinanzasResumenDto();
            }
        }
    }

    public class DashboardAlertasDto
    {
        public List<Producto> ProductosBajoStock { get; set; } = new();
    }
}
