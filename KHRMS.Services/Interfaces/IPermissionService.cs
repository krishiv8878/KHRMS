using System.Collections.Generic;
using System.Threading.Tasks;
using KHRMS.Core.Models;
using KHRMS.Services.Request;

namespace KHRMS.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<IEnumerable<PermissionMaster>> GetAllPermissionsAsync();
        Task<List<long>> GetRolePermissionIdsAsync(long roleId);
        Task<List<RolePermissionViewDto>> GetRolePermissionsWithScopeAsync(long roleId);
        Task<bool> SaveRolePermissionsAsync(long roleId, List<long> permissionIds);
        Task<bool> SaveRolePermissionsWithScopeAsync(long roleId, List<RolePermissionAssignmentDto> permissions, string reason);
        Task<List<string>> GetUserEffectivePermissionsAsync(long employeeId);
        Task<string> GetEffectiveScopeAsync(long employeeId, string permissionCode);
        Task<bool> HasPermissionAsync(long employeeId, string permissionCode);
        Task SeedDefaultPermissionsAsync();
    }
}
