using KHRMS.Core;
using System.Security.Claims;

namespace KHRMS.Services
{
    public interface IAttendanceRequestService
    {
        Task<IEnumerable<AttendanceRequest>> GetAllAsync();
        Task<AttendanceRequest> GetByIdAsync(long id);
        //Task AddAsync(AttendanceRequest attendanceRequest);
        Task AddAsync(AttendanceRequest attendanceRequest, ClaimsPrincipal user);
        Task UpdateAsync(AttendanceRequest attendanceRequest);
        Task DeleteAsync(long id);

    }
}
