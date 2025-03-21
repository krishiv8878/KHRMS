using KHRMS.Core;

namespace KHRMS.Services
{
    public class LeaveTypeService(IUnitOfWork unitOfWork) : ILeaveTypeService
    {
        public IUnitOfWork _unitOfWork = unitOfWork;
        public async Task<bool> AddLeaveType(LeaveType leaveType)
        {
            if (leaveType != null)
            {
                leaveType.CreatedDate = DateTime.Now;
                await _unitOfWork.LeaveType.Add(leaveType);

                var result = _unitOfWork.Save();

                if (result > 0)
                    return true;
                else
                    return false;
            }
            return false;
        }

        

        public async Task<bool> DeleteLeaveType(long LeaveTypeId)
        {
            if (LeaveTypeId > 0)
            {
                var leaveTypeDetail = await _unitOfWork.LeaveType.GetById(LeaveTypeId);
                if (leaveTypeDetail != null)
                {
                    leaveTypeDetail.IsDeleted = true;
                    leaveTypeDetail.IsActive = false;

                    _unitOfWork.LeaveType.Update(leaveTypeDetail);
                    var result = _unitOfWork.Save();
                    if (result > 0)
                        return true;
                    else
                        return false;
                       
                }
            }
            return false;
        }

        public async Task<IEnumerable<LeaveType>> GetAllLeaveType()
        {
            var leaveTypeDetail = await _unitOfWork.LeaveType.GetAll();
            return leaveTypeDetail;
        }

        public async Task<LeaveType> GetLeaveTypeById(int LeaveTypeId)
        {
            if (LeaveTypeId > 0)
            {
                var leaveTypeDetail = await _unitOfWork.LeaveType.GetById(LeaveTypeId);
                if (leaveTypeDetail != null)
                {
                    return leaveTypeDetail;
                }
            }
            return null;
        }

        public async Task<bool> UpdateLeaveType(LeaveType leaveType)
        {
            if (leaveType != null)
            {
                var leaveTypeDetail = await _unitOfWork.LeaveType.GetById(leaveType.Id);
                if (leaveTypeDetail != null)
                {
                    //leaveTypeDetail.LeaveName = leaveType.LeaveName;
                    leaveTypeDetail.Description = leaveType.Description;
                    leaveTypeDetail.Type = leaveType.Type;
                    leaveTypeDetail.UpdatedDate = DateTime.Now;
                    // ✅ Ensure IsActive status is updated
                    leaveTypeDetail.IsActive = leaveType.IsActive;
                    _unitOfWork.LeaveType.Update(leaveTypeDetail);
                    var result = _unitOfWork.Save();
                    if (result > 0)
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }
    }
}
