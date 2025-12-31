
using KHRMS.Core;
using KHRMS.Core.Models;

namespace KHRMS.Services.Interfaces
{
    public interface IAttendanceLogService
    {
        Task<IEnumerable<AttendanceLog>> GetAllAttendanceLogAsync();
        Task AddAttendanceLogAsync(AttendanceLog attendanceLog);
        Task UpdateAttendanceLogAsync(AttendanceLog attendanceLog);
        Task DeleteAttendanceLogAsync(long id);

        Task<AttendanceLog> GetAttendanceLogByIdAsync (long id);

    }
}
