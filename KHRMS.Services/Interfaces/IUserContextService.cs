using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KHRMS.Services.Interfaces
{
    public interface IUserContextService
    {
        long GetCurrentEmployeeId();
        List<string> GetCurrentUserRoles();
        bool IsInRole(string role);
        bool IsAdmin();
        bool IsHR();
        bool IsManager();
        Task<bool> HasPermissionAsync(string permissionCode);
        Task<List<string>> GetUserPermissionsAsync();
    }
}
