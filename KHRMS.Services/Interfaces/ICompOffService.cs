using KHRMS.Core.Models;

namespace KHRMS.Services.Interfaces
{
    public class CompOffEligibleDateDto
    {
        public DateTime AttendanceDate { get; set; }
        public string DayType { get; set; } = string.Empty; // Weekend or Holiday
        public string? HolidayName { get; set; }
        public int WorkedMinutes { get; set; }
        public decimal EligibleDays { get; set; } // 0.5 or 1.0
        public bool AlreadyRequested { get; set; }
        public string? RequestStatus { get; set; }
    }

    public interface ICompOffService
    {
        Task<IEnumerable<CompOffEligibleDateDto>> GetEligibleAttendanceDatesAsync(long employeeId);
        Task<CompOffGrantRequest> ApplyCompOffGrantAsync(CompOffGrantRequest request);
        Task<IEnumerable<CompOffGrantRequest>> GetAllCompOffGrantRequestsAsync();
        Task<bool> ApproveCompOffGrantAsync(long requestId, long approverId);
        Task<bool> RejectCompOffGrantAsync(long requestId, long approverId, string rejectionReason);
    }
}
