using KHRMS.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KHRMS.Services
{
    public interface IEmployeeRoleMappingService
    {
        Task<bool> CreateEmployeeRoleMapping(EmployeeRoleMapping employeeRoleMapping);
        Task<IEnumerable<EmployeeRoleMapping>> GetAllEmployeeRoleMapping();
        Task<EmployeeRoleMapping?> GetRoleMappingById(long employeeRoleMappingId);
        Task<bool> UpdateEmployeeRoleMapping(EmployeeRoleMapping employeeRoleMapping);
        Task<bool> DeleteEmployeeRoleMapping(long employeeRoleMappingId);
        Task<bool> AssignRolesToEmployeeAsync(long targetEmployeeId, List<long> roleIds, string reason);
    }
}
