using KHRMS.Core;

namespace KHRMS.Services
{
    public interface IEmployeeAttendanceService
    {
        Task<EmployeeAttendance> GetByEmployeeIdAsync(long employeeId);

        Task<IEnumerable<EmployeeAttendance>> GetAllAsync();
        Task<EmployeeAttendance> GetByIdAsync(long id);
        Task AddAsync(EmployeeAttendance attendance);

        Task UpdateAsync(EmployeeAttendance attendance);
        Task DeleteAsync(long id);

        Task UpdateExistingAsync(EmployeeAttendance attendance,EmployeeAttendance attendancebyid);

        Task<bool> SendRegularizationRequestEmail(EmployeeAttendance attendance);
        Task<bool> ApproveRegularizationRequestAsync(EmployeeAttendance attendance);

    }
}

