using System.Collections.Generic;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Services.Request;

namespace KHRMS.Services
{
    public interface IResignationService
    {
        Task<bool> AddResignations(ResignationRequestModel resignation);
        Task<IEnumerable<ResignationRequestModel>> GetAllResignations();
        Task<Resignation?> GetResignationById(int resignationId);
        Task<bool> UpdateResignation(Resignation resignation);
        Task<bool> DeleteResignation(long ResignationId);
        Task<bool> ApproveOrRejectResignation(Resignation resignation);

        // State Machine & Offboarding Operations
        Task<bool> WithdrawResignationAsync(long resignationId);
        Task<bool> ManagerReviewResignationAsync(long resignationId, ManagerResignationReviewDto dto);
        Task<bool> HrApproveResignationAsync(long resignationId, HrResignationApprovalDto dto);
        Task<bool> FinalizeEmployeeExitAsync(long resignationId, FinalizeExitDto dto);
        Task<bool> RehireEmployeeAsync(long employeeId, RehireEmployeeDto dto);
        Task<bool> UpdateFnFStatusAsync(long resignationId, FnFStatusUpdateDto dto);
        Task<int> GetOverdueExitsCountAsync();
        Task<IEnumerable<Resignation>> GetOverdueExitsAsync();
    }
}
