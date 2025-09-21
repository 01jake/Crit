using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Crit.Shared.Models;

namespace Crit.Client.Services
{
    public class QuejaPublicaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<QuejaPublicaService> _logger;

        public QuejaPublicaService(HttpClient httpClient, ILogger<QuejaPublicaService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // devuelve éxito y posible mensaje de error del servidor
        public async Task<(bool Success, string? ErrorMessage)> EnviarQuejaPublicaAsync(QuejaPublica queja)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Quejas/publica", queja);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error al enviar queja pública. StatusCode: {Status}, Body: {Body}", response.StatusCode, content);
                return (false, content);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de red al enviar queja pública");
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al enviar queja pública");
                return (false, "Error inesperado.");
            }
        }
    }
}