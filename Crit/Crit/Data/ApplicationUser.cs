using Crit.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace Crit.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public int? EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }

        public string? NombreCompleto { get; set; }
    }

}
