using KHRMS.Core.Models;
using KHRMS.Core;

namespace KHRMS.Services.Interfaces
{
    public interface ILopConversionService
    {
        Task<IEnumerable<LeaveRequest>> GetEligibleLopRequestsAsync(long employeeId);
        Task<LopConversionRequest> ApplyConversionAsync(LopConversionRequest request);
        Task<IEnumerable<LopConversionRequest>> GetAllConversionRequestsAsync();
        Task<bool> ApproveConversionAsync(long conversionRequestId, long approverId);
        Task<bool> RejectConversionAsync(long conversionRequestId, long approverId, string rejectionReason);
    }
}
