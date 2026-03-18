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
                var stats = await _httpClient.GetFromJsonAsync<DashboardStatsDto>("api/dashboard/stats");
                return stats ?? new DashboardStatsDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas");
                return new DashboardStatsDto();
            }
        }

        public async Task<List<VentasPorMesDto>> GetVentasPorMesAsync(int meses = 6)
        {
            try
            {
                var ventas = await _httpClient.GetFromJsonAsync<List<VentasPorMesDto>>(
                    $"api/dashboard/ventas-por-mes?meses={meses}");
                return ventas ?? new List<VentasPorMesDto>();
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
                var productos = await _httpClient.GetFromJsonAsync<List<ProductoMasVendidoDto>>(
                    $"api/dashboard/productos-mas-vendidos?cantidad={cantidad}");
                return productos ?? new List<ProductoMasVendidoDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos más vendidos");
                return new List<ProductoMasVendidoDto>();
            }
        }

        public async Task<DashboardAlertasDto> GetAlertasAsync()
        {
            try
            {
                var alertas = await _httpClient.GetFromJsonAsync<DashboardAlertasDto>("api/dashboard/alertas");
                return alertas ?? new DashboardAlertasDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener alertas");
                return new DashboardAlertasDto();
            }
        }
    }

    // DTO para alertas
    public class DashboardAlertasDto
    {
        public List<Producto> ProductosBajoStock { get; set; } = new();
    }
}

