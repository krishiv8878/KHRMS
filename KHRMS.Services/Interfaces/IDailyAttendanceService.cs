using KHRMS.Core.Models;

namespace KHRMS.Services.Interfaces
{
    public class DailyAttendanceRecordDTO
    {
        public long Id { get; set; }
        public long EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string AttendanceDate { get; set; } = string.Empty;
        public long? ShiftId { get; set; }
        public string ShiftName { get; set; } = string.Empty;
        public DateTime? FirstClockIn { get; set; }
        public DateTime? LastClockOut { get; set; }
        public int TotalWorkMinutes { get; set; }
        public int TotalBreakMinutes { get; set; }
        public int EffectiveWorkMinutes { get; set; }
        public int OvertimeMinutes { get; set; }
        public int LateArrivalMinutes { get; set; }
        public int EarlyExitMinutes { get; set; }
        public string Status { get; set; } = "Absent";
        public long? LeaveRequestId { get; set; }
        public bool IsRegularized { get; set; }
        public long? AttendanceLockId { get; set; }
        public string? Remarks { get; set; }
    }

    public class AttendanceMonthlySummaryDTO
    {
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalDaysInMonth { get; set; }
        public decimal PresentDays { get; set; }
        public decimal HalfDays { get; set; }
        public decimal AbsentDays { get; set; }
        public int LateMarksCount { get; set; }
        public decimal LatePenaltyDays { get; set; }
        public decimal OnLeaveDays { get; set; }
        public decimal WeekOffDays { get; set; }
        public decimal HolidayDays { get; set; }
        public decimal TotalEffectiveHours { get; set; }
        public bool IsMonthLocked { get; set; }
    }

    public interface IDailyAttendanceService
    {
        Task<int> ComputeDailyAttendanceAsync(DateOnly targetDate, long? specificEmployeeId = null);
        Task<int> ComputeDateRangeAsync(DateOnly fromDate, DateOnly toDate, long? specificEmployeeId = null);
        Task<IEnumerable<DailyAttendanceRecordDTO>> GetDailyRecordsAsync(long? employeeId, int month, int year);
        Task<AttendanceMonthlySummaryDTO> GetMonthlySummaryAsync(long employeeId, int month, int year);
        Task<int> RunAutoClockOutAsync();
    }
}
