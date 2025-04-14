using KHRMS.Core;

namespace KHRMS.Services
{
    public interface ILeaveRequestTypeService
    {
        Task<bool> AddLeaveRequestType(LeaveRequest leaveRequest);
        Task<IEnumerable<LeaveRequest>> GetAllLeaveRequestType();
        Task<LeaveRequest> GetLeaveRequestTypeById(int LeaveRequestTypeId);
        Task<bool> UpdateLeaveRequestType(LeaveRequest leaveRequest);
        Task<bool> DeleteLeaveRequestType(long LeaveRequestTypeId);
        Task<bool> ApproveLeaveRequestAsync(int id);


    }
}


  

