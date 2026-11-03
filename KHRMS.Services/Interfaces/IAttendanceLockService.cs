using KHRMS.Core.Models;

namespace KHRMS.Services.Interfaces
{
    public class AttendanceLockDTO
    {
        public long Id { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalEmployeesLocked { get; set; }
        public decimal TotalPresentDays { get; set; }
        public decimal TotalLopDays { get; set; }
        public decimal TotalOvertimeHours { get; set; }
        public bool IsLocked { get; set; }
        public long LockedByEmployeeId { get; set; }
        public string LockedByEmployeeName { get; set; } = string.Empty;
        public DateTime LockedDate { get; set; }
        public string? UnlockedReason { get; set; }
        public long? UnlockedByEmployeeId { get; set; }
        public string? UnlockedByEmployeeName { get; set; }
        public DateTime? UnlockedDate { get; set; }
    }

    public class LockMonthRequest
    {
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class UnlockMonthRequest
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public interface IAttendanceLockService
    {
        Task<IEnumerable<AttendanceLockDTO>> GetAllLocksAsync();
        Task<AttendanceLockDTO?> GetLockForMonthAsync(int month, int year);
        Task<bool> IsMonthLockedAsync(int month, int year);
        Task<AttendanceLockDTO> LockMonthAsync(int month, int year, long lockedByEmployeeId);
        Task<AttendanceLockDTO> UnlockMonthAsync(int month, int year, long unlockedByEmployeeId, string reason);
    }
}
