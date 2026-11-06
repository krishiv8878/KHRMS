using KHRMS.Services.Request;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KHRMS.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync();
        Task<DepartmentDto?> GetDepartmentByIdAsync(long id);
        Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentRequest request, long currentUserId = 0);
        Task<DepartmentDto?> UpdateDepartmentAsync(long id, UpdateDepartmentRequest request, long currentUserId = 0);
        Task<bool> DeleteDepartmentAsync(long id, long currentUserId = 0);
        Task<int> BackfillDepartmentsFromExistingEmployeesAsync();
    }
}
