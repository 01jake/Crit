using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crit.Shared.Models
{
    public interface IEmpresaProvider
    {
        Task<int> GetEmpresaIdAsync();
        string? GetUserId();
    }
}
