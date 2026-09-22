using KHRMS.Core.Models;

namespace KHRMS.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<IEnumerable<PermissionMaster>> GetAllPermissionsAsync();
        Task<List<long>> GetRolePermissionIdsAsync(long roleId);
        Task<bool> SaveRolePermissionsAsync(long roleId, List<long> permissionIds);
        Task<List<string>> GetUserEffectivePermissionsAsync(long employeeId);
        Task<bool> HasPermissionAsync(long employeeId, string permissionCode);
        Task SeedDefaultPermissionsAsync();
    }
}
