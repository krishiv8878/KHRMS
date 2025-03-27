using KHRMS.Core;
using KHRMS.Infrastructure.Migrations;



namespace KHRMS.Services
{
    public class LeaveRequestTypeService(IUnitOfWork unitOfWork, ISendEmailService emailRepository) : ILeaveRequestTypeService
    {
        public IUnitOfWork _unitOfWork = unitOfWork;
        public ISendEmailService _sendEmailService = emailRepository;




        //public async Task<bool> AddLeaveRequestType(LeaveRequest leaveRequest)

        //{

        //    if (leaveRequest != null)

        //    {

        //        leaveRequest.CreatedDate = DateTime.Now;

        //        await _unitOfWork.LeaveRequest.Add(leaveRequest);

        //        var result = _unitOfWork.Save();

        //        if (result > 0)

        //            return true;

        //        else

        //            return false;

        //    }

        //    return false;

        //}

        public async Task<bool> AddLeaveRequestType(LeaveRequest leaveRequest)
        {
            if (leaveRequest != null)
            {
                leaveRequest.CreatedDate = DateTime.Now;
                await _unitOfWork.LeaveRequest.Add(leaveRequest);



                // Fetch Employee details using EmployeeId
                var employee = await _unitOfWork.Employees.GetById(leaveRequest.EmployeeId);

                if (employee == null)
                {
                    throw new Exception("Employee not found.");
                }

                var manager = (await _unitOfWork.Employees.GetAll()).FirstOrDefault(t => t.Id == employee.ManagerId);

                if (manager == null)
                {
                    throw new Exception("Manager not found.");
                }

                // Email parameters from database
                var startDate = new DateTime(2025, 4, 10);
                var endDate = new DateTime(2025, 4, 12);
                var managerEmail = manager.EmailAddress;
                var managerName = $"{manager.FirstName} {manager.LastName}";
                var employeeName = $"{employee.FirstName} {employee.LastName}";
                var leaveReason = "LeaveRequest";

                // Create dictionary for placeholders
                var dict = new Dictionary<string, string>
            {
                { "ManagerName", managerName },
                { "StartDate", startDate.ToString("yyyy-MM-dd") },
                { "EndDate", endDate.ToString("yyyy-MM-dd") },
                { "LeaveReason", leaveReason },
                { "EmployeeName", employeeName },
                { "ManagerEmail", managerEmail }
            };

                // Email subject
                var subject = $"Attendance Request from {employeeName}";

                // Send email
                await _sendEmailService.SendTemplateEmailAsync(managerEmail, subject, dict, leaveReason);

                return true;
            }
            var result = _unitOfWork.Save();

            return false;
        }

        public async Task<IEnumerable<LeaveRequest>> GetAllLeaveRequestType()
        {
            var leaveTypeDetail = await _unitOfWork.LeaveRequest.GetAll();
            return leaveTypeDetail;
        }
        public async Task<LeaveRequest> GetLeaveRequestTypeById(int LeaveRequestTypeId)
        {
            return await unitOfWork.LeaveRequest.GetByIdAsync(LeaveRequestTypeId);
        }

        public async Task<bool> UpdateLeaveRequestType(LeaveRequest leaveRequest)
        {
            if(leaveRequest == null) {
                var leaveTypeDetail = await _unitOfWork.LeaveRequest.GetById(leaveRequest.Id);
                if (leaveTypeDetail != null)
                {

                    leaveTypeDetail.StartDate = leaveRequest.StartDate;
                    leaveTypeDetail.EndDate = leaveRequest.EndDate;
                    leaveTypeDetail.UpdatedDate = DateTime.Now;
                    // ✅ Ensure IsActive status is updated
                    leaveTypeDetail.IsActive = leaveRequest.IsActive;
                    _unitOfWork.LeaveRequest.Update(leaveTypeDetail);
                    var result = _unitOfWork.Save();
                    if (result > 0)
                        return true;
                    else
                        return false;
                }
            }
            return false;

        }

       

        public async Task<bool> DeleteLeaveRequestType(long LeaveRequestTypeId)
        {
            if (LeaveRequestTypeId > 0)
            {
                var leaveTypeDetail = await _unitOfWork.LeaveRequest.GetById(LeaveRequestTypeId);
                if (leaveTypeDetail != null)
                {
                    leaveTypeDetail.IsDeleted = true;
                    leaveTypeDetail.IsActive = false;

                    _unitOfWork.LeaveRequest.Update(leaveTypeDetail);
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
