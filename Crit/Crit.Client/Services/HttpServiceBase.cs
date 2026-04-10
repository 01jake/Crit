using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Crit.Client.Services
{
    public abstract class HttpServiceBase
    {
        protected readonly HttpClient HttpClient;
        protected readonly ILogger Logger;

        protected HttpServiceBase(HttpClient httpClient, ILogger logger)
        {
            HttpClient = httpClient;
            Logger = logger;
        }

        protected async Task<List<T>> GetListAsync<T>(string url)
        {
            try
            {
                var response = await HttpClient.GetAsync(url);

                if (response.StatusCode == HttpStatusCode.Unauthorized ||
                    response.StatusCode == HttpStatusCode.Forbidden)
                    return new List<T>();

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<T>>() ?? new List<T>();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error GET list {Url}", url);
                return new List<T>();
            }
        }

        protected async Task<T?> GetAsync<T>(string url)
        {
            try
            {
                var response = await HttpClient.GetAsync(url);

                if (response.StatusCode == HttpStatusCode.Unauthorized ||
                    response.StatusCode == HttpStatusCode.Forbidden)
                    return default;

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return default;

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error GET {Url}", url);
                return default;
            }
        }

        protected async Task<bool> PostAsync<T>(string url, T data)
        {
            try
            {
                var response = await HttpClient.PostAsJsonAsync(url, data);

                if (response.StatusCode == HttpStatusCode.Unauthorized ||
                    response.StatusCode == HttpStatusCode.Forbidden)
                    return false;

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error POST {Url}", url);
                return false;
            }
        }

        protected async Task<TResponse?> PostAndReadAsync<TRequest, TResponse>(string url, TRequest data)
        {
            try
            {
                var response = await HttpClient.PostAsJsonAsync(url, data);

                if (response.StatusCode == HttpStatusCode.Unauthorized ||
                    response.StatusCode == HttpStatusCode.Forbidden)
                    return default;

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<TResponse>();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error POST/READ {Url}", url);
                return default;
            }
        }

        protected async Task<bool> PutAsync<T>(string url, T data)
        {
            try
            {
                var response = await HttpClient.PutAsJsonAsync(url, data);

                if (response.StatusCode == HttpStatusCode.Unauthorized ||
                    response.StatusCode == HttpStatusCode.Forbidden)
                    return false;

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error PUT {Url}", url);
                return false;
            }
        }

        protected async Task<bool> PutEmptyAsync(string url)
        {
            try
            {
                var response = await HttpClient.PutAsync(url, null);

                if (response.StatusCode == HttpStatusCode.Unauthorized ||
                    response.StatusCode == HttpStatusCode.Forbidden)
                    return false;

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error PUT empty {Url}", url);
                return false;
            }
        }

        protected async Task<bool> DeleteAsync(string url)
        {
            try
            {
                var response = await HttpClient.DeleteAsync(url);

                if (response.StatusCode == HttpStatusCode.Unauthorized ||
                    response.StatusCode == HttpStatusCode.Forbidden)
                    return false;

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error DELETE {Url}", url);
                return false;
            }
        }
    }
}
