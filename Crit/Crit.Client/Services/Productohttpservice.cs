using Crit.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Crit.Client.Services
{
    public class ProductoHttpService : HttpServiceBase
    {
        public ProductoHttpService(HttpClient httpClient, ILogger<ProductoHttpService> logger)
            : base(httpClient, logger)
        {
        }

        public Task<List<Producto>> GetProductosAsync()
     => base.GetListAsync<Producto>("api/productos");

        public Task<Producto?> GetProductoAsync(int id)
            => base.GetAsync<Producto>($"api/productos/{id}");

        public Task<List<Producto>> GetProductosActivosAsync()
            => base.GetListAsync<Producto>("api/productos/activos");

        public Task<List<Producto>> GetProductosBajoStockAsync()
            => base.GetListAsync<Producto>("api/productos/bajo-stock");

        public Task<bool> UpdateProductoAsync(Producto producto)
            => base.PutAsync($"api/productos/{producto.Id}", producto);

        public Task<bool> DeleteProductoAsync(int id)
            => base.DeleteAsync($"api/productos/{id}");

        public Task<bool> ActualizarStockAsync(int id, int cantidad)
            => base.PutAsync($"api/productos/{id}/stock", cantidad);

        public Task<Producto?> CreateProductoAsync(Producto producto)
            => base.PostAndReadAsync<Producto, Producto>("api/productos", producto);

    }
}
