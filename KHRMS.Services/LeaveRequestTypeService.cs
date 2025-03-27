using KHRMS.Core;
using KHRMS.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static System.Runtime.InteropServices.JavaScript.JSType;



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
                var leaverequest = new LeaveRequest
                {
                    Id = leaveRequest.Id,
                    EmployeeId = leaveRequest.EmployeeId,
                    StartDate = leaveRequest.StartDate,
                    EndDate = leaveRequest.EndDate,
                    CreatedDate = DateTime.Now,
                    IsActive = leaveRequest.IsActive,
                    IsApproved = false,
                    IsDeleted = leaveRequest.IsDeleted
                };

                await _unitOfWork.LeaveRequest.Add(leaverequest);



                // Fetch Employee details using EmployeeId
                var employee = await _unitOfWork.Employees.GetById(leaverequest.EmployeeId);

                if (employee == null)
                {
                    throw new Exception("Employee not found.");
                }

                var manager = (await _unitOfWork.Employees.GetAll()).FirstOrDefault(t => t.Id == employee.ManagerId);

                if (manager == null)
                {
                    throw new Exception("Manager not found.");
                }
                var result = _unitOfWork.Save();

                // Email parameters from database
                var startDate = new DateTime(2025, 4, 10);
                var endDate = new DateTime(2025, 4, 12);
                string formattedStartDate = GetFormattedDate(startDate);
                string formattedEndDate = GetFormattedDate(endDate);
                var managerEmail = manager.EmailAddress;
                var managerName = $"{manager.FirstName} {manager.LastName}";
                var employeeName = $"{employee.FirstName} {employee.LastName}";
                var leaveReason = "LeaveRequest";

                // Create dictionary for placeholders
                var dict = new Dictionary<string, string>
            {
                { "ManagerName", managerName },
                { "StartDate", formattedStartDate  },
                { "EndDate", formattedEndDate },
                { "LeaveReason", leaveReason },
                { "EmployeeName", employeeName },
                { "ManagerEmail", managerEmail }
            };
                // Function to format date with proper ordinal suffix
                string GetFormattedDate(DateTime date)
                {
                    int day = date.Day;
                    string suffix = (day % 10 == 1 && day != 11) ? "st"
                                 : (day % 10 == 2 && day != 12) ? "nd"
                                 : (day % 10 == 3 && day != 13) ? "rd"
                                 : "th";

                    return $"{day}{suffix} {date:MMMM yyyy}";
                }
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
            if (leaveRequest == null)
            {
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
