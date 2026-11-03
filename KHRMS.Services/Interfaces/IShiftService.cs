using System.Collections.Generic;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Models;

namespace KHRMS.Services
{
    public interface IShiftService
    {
        Task<IEnumerable<ShiftMaster>> GetAllShiftsAsync();
        Task<ShiftMaster> GetShiftByIdAsync(long id);
        Task AddShiftAsync(ShiftMaster shiftMaster);
        Task UpdateShiftAsync(ShiftMaster shift);
        Task DeleteShiftAsync(long id);

        // Phase 4: Shift Assignments & Rosters
        Task<IEnumerable<EmployeeShiftAssignment>> GetEmployeeShiftAssignmentsAsync(long employeeId);
        Task<EmployeeShiftAssignment> AssignShiftToEmployeeAsync(EmployeeShiftAssignment assignment);
    }
}
