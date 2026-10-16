using KHRMS.Core;
using KHRMS.Services.Request;
using System.Security.Claims;

namespace KHRMS.Services
{
    public interface IAttendanceRequestService
    {
        Task<IEnumerable<AttendanceRequestUpdateDTO>> GetAllAsync();
        Task<AttendanceRequest> GetByIdAsync(long id);
        //Task AddAsync(AttendanceRequest attendanceRequest);
        Task AddAsync(AttendanceRequestDTO attendanceRequest, ClaimsPrincipal user);
        Task UpdateAsync(AttendanceRequestUpdateDTO attendanceRequest);
        Task DeleteAsync(long id);
        Task<bool> SetDailyWorkModeAsync(DailyWorkModeDTO dto, ClaimsPrincipal user);
        Task<IEnumerable<EmployeeWorkModeDTO>> GetDailyWorkModesAsync(DateTime? startDate = null, DateTime? endDate = null, long? employeeId = null);
    }
}
