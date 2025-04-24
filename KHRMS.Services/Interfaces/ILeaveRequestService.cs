using KHRMS.Core;
using KHRMS.Services.Request;

namespace KHRMS.Services
{
    public interface ILeaveRequestTypeService
    {
        Task<bool> AddLeaveRequestType(LeaveRequest leaveRequest);
        Task<IEnumerable<LeaveReqestModel>> GetAllLeaveRequestType();
        Task<LeaveRequest> GetLeaveRequestTypeById(int LeaveRequestTypeId);
        Task<bool> UpdateLeaveRequestType(LeaveRequest leaveRequest);
        Task<bool> DeleteLeaveRequestType(long LeaveRequestTypeId);
        Task<bool> ApproveLeaveRequestAsync(LeaveRequest leaveRequest);



    }
}




