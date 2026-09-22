using KHRMS.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace KHRMS.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;

        public UserContextService(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public long GetCurrentEmployeeId()
        {
            var id = User?.FindFirst("UserId")?.Value
                  ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User?.FindFirst("sub")?.Value
                  ?? User?.FindFirst("id")?.Value;
            return (id != null && long.TryParse(id, out var parsed)) ? parsed : 0;
        }

        public List<string> GetCurrentUserRoles()
        {
            if (User == null) return new List<string>();

            return User.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .Union(User.FindAll("role").Select(c => c.Value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public bool IsInRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role) || User == null) return false;
            return User.IsInRole(role) || GetCurrentUserRoles().Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsAdmin() => IsInRole("Admin") || IsInRole("System Admin");
        public bool IsHR() => IsInRole("HR") || IsInRole("HR Operations");
        public bool IsManager() => IsInRole("Manager") || IsInRole("Management");

        public async Task<bool> HasPermissionAsync(string permissionCode)
        {
            if (IsAdmin()) return true;
            var empId = GetCurrentEmployeeId();
            if (empId <= 0) return false;

            using var scope = _serviceProvider.CreateScope();
            var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
            return await permissionService.HasPermissionAsync(empId, permissionCode);
        }

        public async Task<List<string>> GetUserPermissionsAsync()
        {
            var empId = GetCurrentEmployeeId();
            if (empId <= 0) return new List<string>();

            using var scope = _serviceProvider.CreateScope();
            var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
            return await permissionService.GetUserEffectivePermissionsAsync(empId);
        }
    }
}
