using System.Collections.Generic;
using System.Threading.Tasks;

namespace KHRMS.Services.Interfaces
{
    public interface IEmployeeAccessService
    {
        /// <summary>
        /// Validates whether the currently authenticated user is authorized to access or manage
        /// records belonging to targetEmployeeId based on legacy or default rules.
        /// </summary>
        Task<bool> CanAccessEmployeeAsync(long targetEmployeeId, string? privilegedPermissionCode = null);

        /// <summary>
        /// Validates access based on the Strict Scope Hierarchy for a specific permission code:
        /// Self < Team < TeamAndIndirect < Department < Branch < All.
        /// </summary>
        Task<bool> CanAccessEmployeeWithScopeAsync(long targetEmployeeId, string permissionCode);

        /// <summary>
        /// Retrieves all employee IDs that fall within the current authenticated user's scope
        /// for the specified permission code.
        /// </summary>
        Task<List<long>> GetInScopeEmployeeIdsAsync(string permissionCode);

        /// <summary>
        /// Retrieves the highest resolved scope name for the authenticated user and given permission.
        /// </summary>
        Task<string> GetEffectiveScopeForPermissionAsync(string permissionCode);

        /// <summary>
        /// Validates whether targetEmployeeId reports directly or indirectly to managerEmployeeId.
        /// </summary>
        Task<bool> IsInReportingHierarchyAsync(long managerEmployeeId, long targetEmployeeId);

        /// <summary>
        /// Retrieves all employee IDs that report directly or indirectly to managerEmployeeId.
        /// </summary>
        Task<List<long>> GetSubordinateEmployeeIdsAsync(long managerEmployeeId);

        /// <summary>
        /// Returns the authenticated employee ID directly from JWT claims.
        /// </summary>
        long GetCurrentEmployeeId();
    }
}
