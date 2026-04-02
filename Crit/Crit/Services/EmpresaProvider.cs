using System.Security.Claims;
using Crit.Data;
using Crit.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace Crit.Client.Services
{
    public class EmpresaProvider : IEmpresaProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmpresaProvider(
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public string? GetUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public async Task<int> GetEmpresaIdAsync()
        {
            var userId = GetUserId();

            if (string.IsNullOrWhiteSpace(userId))
                return 0;

            var user = await _userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId);

            return user?.EmpresaId ?? 0;
        }
    }
}
