using Crit.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Crit.Client.Services
{
    public class ProveedorHttpService : HttpServiceBase
    {
        public ProveedorHttpService(HttpClient http, ILogger<ProveedorHttpService> logger)
            : base(http, logger)
        {
        }

        public Task<List<Proveedor>> GetAsync()
            => base.GetListAsync<Proveedor>("api/proveedores");

        public Task<Proveedor?> GetByIdAsync(int id)
            => base.GetAsync<Proveedor>($"api/proveedores/{id}");

        public Task<bool> CreateAsync(Proveedor proveedor)
            => base.PostAsync("api/proveedores", proveedor);

        public Task<bool> UpdateAsync(Proveedor proveedor)
            => base.PutAsync($"api/proveedores/{proveedor.Id}", proveedor);

        public Task<bool> DeleteAsync(int id)
            => base.DeleteAsync($"api/proveedores/{id}");
    }
}
