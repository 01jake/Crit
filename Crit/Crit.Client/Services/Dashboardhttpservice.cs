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
                return await _httpClient.GetFromJsonAsync<DashboardStatsDto>("api/dashboard/stats")
                       ?? new DashboardStatsDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener stats");
                return new DashboardStatsDto();
            }
        }

        public async Task<List<CashFlowDto>> GetCashFlowAsync(int meses = 6)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<CashFlowDto>>($"api/dashboard/cash-flow?meses={meses}")
                       ?? new List<CashFlowDto>();
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
                return await _httpClient.GetFromJsonAsync<List<VentasPorDiaDto>>($"api/dashboard/ventas-por-dia?dias={dias}")
                       ?? new List<VentasPorDiaDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas por día");
                return new List<VentasPorDiaDto>();
            }
        }

        public async Task<List<ProductoMasVendidoDto>> GetProductosMasVendidosAsync(int cantidad = 5)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<ProductoMasVendidoDto>>($"api/dashboard/productos-mas-vendidos?cantidad={cantidad}")
                       ?? new List<ProductoMasVendidoDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener top productos");
                return new List<ProductoMasVendidoDto>();
            }
        }

        public async Task<DashboardAlertaDto> GetAlertasAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<DashboardAlertaDto>("api/dashboard/alertas")
                       ?? new DashboardAlertaDto();
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
                return await _httpClient.GetFromJsonAsync<List<VentaRecienteDto>>($"api/dashboard/ventas-recientes?cantidad={cantidad}")
                       ?? new List<VentaRecienteDto>();
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
                // Forzamos el formato yyyy-MM-dd para evitar confusiones entre día y mes
                var url = fechaInicio.HasValue
                    ? $"api/dashboard/stats?fechaInicio={fechaInicio.Value:yyyy-MM-dd}"
                    : "api/dashboard/stats";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<DashboardStatsDto>() ?? new DashboardStatsDto();
                }

                // Si hay error, leemos qué dice el servidor para depurar
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error del servidor: {errorContent}");
                return new DashboardStatsDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de conexión en GetStatsAsync");
                return new DashboardStatsDto();
            }
        }

    }
    // DTO para alertas
    public class DashboardAlertasDto
    {
        public List<Producto> ProductosBajoStock { get; set; } = new();
    }

}

