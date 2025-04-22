//using KHRMS.Core;
//using KHRMS.Infrastructure.Migrations;
//using Microsoft.AspNetCore.Http;
//using Microsoft.EntityFrameworkCore.Metadata.Internal;
//using static System.Runtime.InteropServices.JavaScript.JSType;



//namespace KHRMS.Services
//{

//    public class LeaveRequestTypeService(IUnitOfWork unitOfWork, ISendEmailService emailRepository, IHttpContextAccessor httpContextAccessor) : ILeaveRequestTypeService
//    {
//        public IUnitOfWork _unitOfWork = unitOfWork;
//        public ISendEmailService _sendEmailService = emailRepository;
//        private readonly IHttpContextAccessor _httpContextAccessor;





//        public async Task<bool> AddLeaveRequestType(LeaveRequest leaveRequest)
//        {
//            if (leaveRequest != null)
//            {
//                var employeeIdString = _httpContextAccessor.HttpContext.Session.GetString("EmployeeId");
//                if (string.IsNullOrEmpty(employeeIdString))
//                {
//                    throw new Exception("User is not logged in.");
//                }

//                long employeeId = Convert.ToInt64(employeeIdString);

//                var leaverequest = new LeaveRequest
//                {
//                    // Id = leaveRequest.Id,
//                    Id = employeeId,
//                    EmployeeId = leaveRequest.EmployeeId,
//                    StartDate = leaveRequest.StartDate,
//                    EndDate = leaveRequest.EndDate,
//                    CreatedDate = DateTime.Now,
//                    IsActive = leaveRequest.IsActive,
//                    IsApproved = false,
//                    IsDeleted = leaveRequest.IsDeleted
//                };

//                await _unitOfWork.LeaveRequest.Add(leaverequest);



//                // Fetch Employee details using EmployeeId
//                var employee = await _unitOfWork.Employees.GetById(leaverequest.EmployeeId);

//                if (employee == null)
//                {
//                    throw new Exception("Employee not found.");
//                }

//                var manager = (await _unitOfWork.Employees.GetAll()).FirstOrDefault(t => t.Id == employee.ManagerId);

//                if (manager == null)
//                {
//                    throw new Exception("Manager not found.");
//                }
//                var result = _unitOfWork.Save();

//                // Email parameters from database
//                var startDate = new DateTime(2025, 4, 10);
//                var endDate = new DateTime(2025, 4, 12);
//                string formattedStartDate = GetFormattedDate(startDate);
//                string formattedEndDate = GetFormattedDate(endDate);
//                var managerEmail = manager.EmailAddress;
//                var managerName = $"{manager.FirstName} {manager.LastName}";
//                var employeeName = $"{employee.FirstName} {employee.LastName}";
//                var leaveReason = "LeaveRequest";

//                // Create dictionary for placeholders
//                var dict = new Dictionary<string, string>
//            {
//                { "ManagerName", managerName },
//                { "StartDate", formattedStartDate  },
//                { "EndDate", formattedEndDate },
//                { "LeaveReason", leaveReason },
//                    { "EmployeeName", employeeName },
//                    { "ManagerEmail", managerEmail }
//            };
//                // Function to format date with proper ordinal suffix
//                string GetFormattedDate(DateTime date)
//                {
//                    int day = date.Day;
//                    string suffix = (day % 10 == 1 && day != 11) ? "st"
//                                 : (day % 10 == 2 && day != 12) ? "nd"
//                                 : (day % 10 == 3 && day != 13) ? "rd"
//                                 : "th";

//                    return $"{day}{suffix} {date:MMMM yyyy}";
//                }
//                // Email subject
//                var subject = $"Attendance Request from {employeeName}";

//                // Send email
//                await _sendEmailService.SendTemplateEmailAsync(managerEmail, subject, dict, leaveReason);

//                return true;
//            }

//            return false;
//        }

//        public async Task<IEnumerable<LeaveRequest>> GetAllLeaveRequestType()
//        {
//            var leaveTypeDetail = await _unitOfWork.LeaveRequest.GetAll();
//            return leaveTypeDetail;
//        }
//        public async Task<LeaveRequest> GetLeaveRequestTypeById(int LeaveRequestTypeId)
//        {
//            return await unitOfWork.LeaveRequest.GetByIdAsync(LeaveRequestTypeId);
//        }

//        public async Task<bool> UpdateLeaveRequestType(LeaveRequest leaveRequest)
//        {
//            if (leaveRequest == null)
//            {
//                var leaveTypeDetail = await _unitOfWork.LeaveRequest.GetById(leaveRequest.Id);
//                if (leaveTypeDetail != null)
//                {

//                    leaveTypeDetail.StartDate = leaveRequest.StartDate;
//                    leaveTypeDetail.EndDate = leaveRequest.EndDate;
//                    leaveTypeDetail.UpdatedDate = DateTime.Now;
//                    // ✅ Ensure IsActive status is updated
//                    leaveTypeDetail.IsActive = leaveRequest.IsActive;
//                    _unitOfWork.LeaveRequest.Update(leaveTypeDetail);
//                    var result = _unitOfWork.Save();
//                    if (result > 0)
//                        return true;
//                    else
//                        return false;
//                }
//            }
//            return false;

//        }



//        public async Task<bool> DeleteLeaveRequestType(long LeaveRequestTypeId)
//        {
//            if (LeaveRequestTypeId > 0)
//            {
//                var leaveTypeDetail = await _unitOfWork.LeaveRequest.GetById(LeaveRequestTypeId);
//                if (leaveTypeDetail != null)
//                {
//                    leaveTypeDetail.IsDeleted = true;
//                    leaveTypeDetail.IsActive = false;

//                    _unitOfWork.LeaveRequest.Update(leaveTypeDetail);
//                    var result = _unitOfWork.Save();
//                    if (result > 0)
//                        return true;
//                    else
//                        return false;

//                }
//            }
//            return false;
//        }


//    }
//}
using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KHRMS.Services
{
    public class LeaveRequestTypeService : ILeaveRequestTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISendEmailService _sendEmailService;
        private readonly IUserContextService _userContext;

        public LeaveRequestTypeService(
      IUnitOfWork unitOfWork,
      ISendEmailService emailRepository,
      IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _sendEmailService = emailRepository;
            _userContext = userContextService;
        }

        public async Task<bool> AddLeaveRequestType(LeaveRequest leaveRequest)
        {
            if (leaveRequest == null)
                return false;
            long employeeId = _userContext.GetCurrentEmployeeId(); 



            var leaverequest = new LeaveRequest
            {
                EmployeeId = employeeId,
                StartDate = leaveRequest.StartDate,
                EndDate = leaveRequest.EndDate,
                CreatedDate = DateTime.Now,
                IsActive = leaveRequest.IsActive,
                IsApproved = false,
                IsDeleted = leaveRequest.IsDeleted,
                LeaveReason = leaveRequest.LeaveReason,
            };

            await _unitOfWork.LeaveRequest.Add(leaverequest);

            var employee = await _unitOfWork.Employees.GetById(employeeId);
            if (employee == null)
                throw new Exception("Employee not found.");

            var manager = (await _unitOfWork.Employees.GetAll())
                          .FirstOrDefault(t => t.Id == employee.ManagerId);
            if (manager == null)
                throw new Exception("Manager not found.");

            _unitOfWork.Save();

            // Format dates for email
            string formattedStartDate = GetFormattedDate(leaverequest.StartDate);
            string formattedEndDate = GetFormattedDate(leaverequest.EndDate);
            var managerEmail = manager.EmailAddress;
            var managerName = $"{manager.FirstName} {manager.LastName}";
            var employeeName = $"{employee.FirstName} {employee.LastName}";
            var leavetype = "LeaveRequest";

            // Create email placeholders
            var dict = new Dictionary<string, string>
                {
                    { "ManagerName", managerName },
                    { "StartDate", formattedStartDate },
                    { "EndDate", formattedEndDate },
                    { "LeaveType", leavetype },
                    { "EmployeeName", employeeName },
                    { "ManagerEmail", managerEmail },
                     { "LeaveDescription" ,leaveRequest.LeaveReason}
                };

            var subject = $"Leave Request from {employeeName}";

            await _sendEmailService.SendTemplateEmailAsync(managerEmail, subject, dict, leavetype);

            return true;
        }


        public async Task<bool> ApproveLeaveRequestAsync(int id)
        {
            var leaveRequest = await _unitOfWork.LeaveRequest.GetByIdAsync(id);

            if (leaveRequest == null || leaveRequest.Status == "Approved")
                return false;

            leaveRequest.Status = "Approved";
            leaveRequest.ApprovedDate = DateTime.UtcNow;

            _unitOfWork.LeaveRequest.Update(leaveRequest);
            return true;
        }


        private string GetFormattedDate(DateTime date)
        {
            int day = date.Day;
            string suffix = (day % 10 == 1 && day != 11) ? "st"
                         : (day % 10 == 2 && day != 12) ? "nd"
                         : (day % 10 == 3 && day != 13) ? "rd"
                         : "th";

            return $"{day}{suffix} {date:MMMM yyyy}";
        }

        public async Task<IEnumerable<LeaveRequest>> GetAllLeaveRequestType()
        {

             return await _unitOfWork.LeaveRequest.GetAll();
           

        }

        public async Task<LeaveRequest> GetLeaveRequestTypeById(int LeaveRequestTypeId)
        {
            return await _unitOfWork.LeaveRequest.GetByIdAsync(LeaveRequestTypeId);
        }

        public async Task<bool> UpdateLeaveRequestType(LeaveRequest leaveRequest)
        {
            if (leaveRequest == null) return false;

            var existingRequest = await _unitOfWork.LeaveRequest.GetById(leaveRequest.Id);
            if (existingRequest != null)
            {
                existingRequest.StartDate = leaveRequest.StartDate;
                existingRequest.EndDate = leaveRequest.EndDate;
                existingRequest.UpdatedDate = DateTime.Now;
                existingRequest.IsActive = leaveRequest.IsActive;

                _unitOfWork.LeaveRequest.Update(existingRequest);
                return _unitOfWork.Save() > 0;
            }

            return false;
        }
      
        public async Task<bool> DeleteLeaveRequestType(long LeaveRequestTypeId)
        {
            if (LeaveRequestTypeId <= 0) return false;

            var leaveRequest = await _unitOfWork.LeaveRequest.GetById(LeaveRequestTypeId);
            if (leaveRequest != null)
            {
                leaveRequest.IsDeleted = true;
                leaveRequest.IsActive = false;

                _unitOfWork.LeaveRequest.Update(leaveRequest);
                return _unitOfWork.Save() > 0;
            }

            return false;
        }

       
        public async Task<bool> ApproveLeaveRequestAsync(LeaveRequest leaveRequest)
        {

            var leaveRequest1 = await _unitOfWork.LeaveRequest.GetById(leaveRequest.Id);
            if (leaveRequest1 == null || leaveRequest1.IsApproved.GetValueOrDefault()) return false;

            var managerId = _userContext.GetCurrentEmployeeId();

            var employee = await _unitOfWork.Employees.GetById(leaveRequest.EmployeeId);
            var manager = await _unitOfWork.Employees.GetById(managerId);

            if (employee == null || manager == null) return false;

            leaveRequest1.IsApproved = true;
            leaveRequest1.ApprovedDate = DateTime.Now;
            leaveRequest1.ApprovedBy = (int)managerId;

            _unitOfWork.LeaveRequest.Update(leaveRequest1);
            var result = _unitOfWork.Save();

            if (result <= 0) return false;

            var dict = new Dictionary<string, string>
            {
                { "ManagerName", $"{manager.FirstName} {manager.LastName}" },
                { "StartDate", GetFormattedDate(leaveRequest.StartDate) },
                { "EndDate", GetFormattedDate(leaveRequest.EndDate) },
                { "LeaveReason", "Approval Request" },
                { "EmployeeName", $"{employee.FirstName} {employee.LastName}" },
                { "ManagerEmail", manager.EmailAddress }
            };

            string subject = "Your Leave Request Has Been Approved";
            try
            {
                await _sendEmailService.SendTemplateEmailAsync(employee.EmailAddress, subject, dict, "Approval Request");
            }
            catch
            {
                // Optional: log the failure but still return success
            }

            return true;
        }
        public async Task<IEnumerable<LeaveRequest>> GetAllEmployeesLeaveRequest()
        {
            long employeeId = _userContext.GetCurrentEmployeeId(); // 

            var empid = (await _unitOfWork.Employees.GetAll()).Where(r => r.ManagerId == employeeId).Select(r => r.Id).ToList();
            //UpdateLeaveRequestType(empid);
            var leaverequest = (await _unitOfWork.LeaveRequest.GetAll()).Where(r => empid.Contains(r.EmployeeId)).ToList();
            return leaverequest;
        }

    }
}
