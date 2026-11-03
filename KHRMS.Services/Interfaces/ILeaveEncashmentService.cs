using KHRMS.Core.Models;

namespace KHRMS.Services.Interfaces
{
    public class EncashmentEligibilityDto
    {
        public long LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public bool EncashmentAllowed { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal MinBalanceRequired { get; set; }
        public decimal MaxEncashmentDays { get; set; }
        public decimal DailySalaryBasis { get; set; }
        public bool IsEligible { get; set; }
        public string IneligibilityReason { get; set; } = string.Empty;
    }

    public interface ILeaveEncashmentService
    {
        Task<EncashmentEligibilityDto> GetEncashmentEligibilityAsync(long employeeId, long leaveTypeId);
        Task<LeaveEncashmentRequest> ApplyEncashmentAsync(LeaveEncashmentRequest request);
        Task<IEnumerable<LeaveEncashmentRequest>> GetAllEncashmentRequestsAsync();
        Task<bool> ApproveEncashmentAsync(long requestId, long approverId);
        Task<bool> RejectEncashmentAsync(long requestId, long approverId, string rejectionReason);
    }
}
