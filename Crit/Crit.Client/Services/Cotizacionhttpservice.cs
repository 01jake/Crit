using System.Net.Http.Json;
using Crit.Shared.DTOs;
using Crit.Shared.Models;

namespace Crit.Client.Services
{

    public class CotizacionHttpService 
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CotizacionHttpService> _logger;

        public CotizacionHttpService(HttpClient httpClient, ILogger<CotizacionHttpService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<Cotizacion>> GetCotizacionesAsync()
        {
            try
            {
                var cotizaciones = await _httpClient.GetFromJsonAsync<List<Cotizacion>>("api/cotizaciones");
                return cotizaciones ?? new List<Cotizacion>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cotizaciones");
                throw;
            }
        }

        public async Task<Cotizacion?> GetCotizacionAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Cotizacion>($"api/cotizaciones/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cotización {Id}", id);
                throw;
            }
        }

        public async Task<List<Cotizacion>> GetCotizacionesPorClienteAsync(int clienteId)
        {
            try
            {
                var cotizaciones = await _httpClient.GetFromJsonAsync<List<Cotizacion>>($"api/cotizaciones/cliente/{clienteId}");
                return cotizaciones ?? new List<Cotizacion>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cotizaciones del cliente {ClienteId}", clienteId);
                throw;
            }
        }

        public async Task<Cotizacion> CreateCotizacionAsync(Cotizacion cotizacion)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/cotizaciones", cotizacion);
                response.EnsureSuccessStatusCode();
                var cotizacionCreada = await response.Content.ReadFromJsonAsync<Cotizacion>();
                return cotizacionCreada ?? throw new Exception("Error al crear cotización");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cotización");
                throw;
            }
        }

        public async Task UpdateCotizacionAsync(Cotizacion cotizacion)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/cotizaciones/{cotizacion.Id}", cotizacion);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cotización {Id}", cotizacion.Id);
                throw;
            }
        }

        public async Task DeleteCotizacionAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/cotizaciones/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cotización {Id}", id);
                throw;
            }
        }

        public async Task<Venta?> ConvertirAVentaAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/cotizaciones/{id}/convertir-venta", null);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Venta>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al convertir cotización {Id} a venta", id);
                throw;
            }
        }

        public async Task<byte[]> GenerarPdfAsync(int id)
        {
            try
            {
                return await _httpClient.GetByteArrayAsync($"api/cotizaciones/{id}/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de cotización {Id}", id);
                throw;
            }
        }
    }

    // Dashboard Service
    public interface IDashboardHttpService
    {
        Task<DashboardStatsDto> GetStatsAsync();
        Task<List<VentasPorMesDto>> GetVentasPorMesAsync(int meses = 6);
        Task<List<ProductoMasVendidoDto>> GetProductosMasVendidosAsync(int cantidad = 5);
    }

    public class DashboardHttpService : IDashboardHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DashboardHttpService> _logger;

        public DashboardHttpService(HttpClient httpClient, ILogger<DashboardHttpService> logger)
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
                _logger.LogError(ex, "Error al obtener estadísticas del dashboard");
                throw;
            }
        }

        public async Task<List<VentasPorMesDto>> GetVentasPorMesAsync(int meses = 6)
        {
            try
            {
                var ventas = await _httpClient.GetFromJsonAsync<List<VentasPorMesDto>>($"api/dashboard/ventas-por-mes?meses={meses}");
                return ventas ?? new List<VentasPorMesDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas por mes");
                throw;
            }
        }

        public async Task<List<ProductoMasVendidoDto>> GetProductosMasVendidosAsync(int cantidad = 5)
        {
            try
            {
                var productos = await _httpClient.GetFromJsonAsync<List<ProductoMasVendidoDto>>($"api/dashboard/productos-mas-vendidos?cantidad={cantidad}");
                return productos ?? new List<ProductoMasVendidoDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos más vendidos");
                throw;
            }
        }
    }
}
