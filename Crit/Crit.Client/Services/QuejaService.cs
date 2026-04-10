using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Crit.Shared.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace Crit.Client.Services
{
    public class QuejaService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public QuejaService(
            HttpClient httpClient,
            AuthenticationStateProvider authenticationStateProvider)
        {
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<bool> EsAdminAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            return user.Identity?.IsAuthenticated == true && user.IsInRole("Admin");
        }

        public async Task<bool> CreateQuejaAsync(Queja queja)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Quejas", queja);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al crear queja: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Queja>> GetQuejasAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Quejas");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return new List<Queja>();

                response.EnsureSuccessStatusCode();

                var quejas = await response.Content.ReadFromJsonAsync<List<Queja>>();
                return quejas ?? new List<Queja>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al obtener quejas: {ex.Message}");
                return new List<Queja>();
            }
        }

        public async Task<List<Queja>> GetMisQuejasAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Quejas/mis-quejas");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return new List<Queja>();

                response.EnsureSuccessStatusCode();

                var quejas = await response.Content.ReadFromJsonAsync<List<Queja>>();
                return quejas ?? new List<Queja>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al obtener mis quejas: {ex.Message}");
                return new List<Queja>();
            }
        }

        public async Task<List<Queja>> GetMisQuejasAsignadasAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Quejas/mis-asignadas");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return new List<Queja>();

                response.EnsureSuccessStatusCode();

                var quejas = await response.Content.ReadFromJsonAsync<List<Queja>>();
                return quejas ?? new List<Queja>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al obtener quejas asignadas: {ex.Message}");
                return new List<Queja>();
            }
        }

        public async Task<Queja?> GetQuejaByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Quejas/{id}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                    response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return null;

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<Queja>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al obtener queja {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateQuejaStatusAsync(int id, EstatusQueja nuevoEstatus)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/Quejas/{id}/status", nuevoEstatus);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al actualizar estatus: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteQuejaAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/Quejas/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al eliminar queja: {ex.Message}");
                return false;
            }
        }
    }
}
