using Crit.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Crit.Client.Services
{
    public class TraspasoHttpService : HttpServiceBase
    {
        public TraspasoHttpService(HttpClient http, ILogger<TraspasoHttpService> logger)
            : base(http, logger)
        {
        }

        public Task<List<TraspasoAlmacen>> GetAsync()
            => base.GetListAsync<TraspasoAlmacen>("api/traspasos");

        public Task<TraspasoAlmacen?> GetByIdAsync(int id)
            => base.GetAsync<TraspasoAlmacen>($"api/traspasos/{id}");

        public Task<TraspasoAlmacen?> CreateAsync(TraspasoAlmacen traspaso)
            => base.PostAndReadAsync<TraspasoAlmacen, TraspasoAlmacen>("api/traspasos", traspaso);

        public Task<bool> CancelAsync(int id)
            => base.PostAsync<object?>($"api/traspasos/{id}/cancelar", null);
    }
}
