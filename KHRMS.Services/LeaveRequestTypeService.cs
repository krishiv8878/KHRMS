using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace KHRMS.Services
{
    public class LeaveRequestTypeService : ILeaveRequestTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISendEmailService _sendEmailService;
        private readonly IUserContextService _userContext;
        public readonly IHttpContextAccessor _httpContextAccessor;
        public LeaveRequestTypeService(
      IUnitOfWork unitOfWork,
      ISendEmailService emailRepository,
      IUserContextService userContextService,IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _sendEmailService = emailRepository;
            _userContext = userContextService;
            _httpContextAccessor = httpContextAccessor;

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
                IsDeleted = leaveRequest.IsDeleted,
                LeaveReason = leaveRequest.LeaveReason,
                LeaveMode = leaveRequest.LeaveMode,
                LeaveTypeId = leaveRequest.LeaveTypeId,
                Status = "Pending"
            };

            await _unitOfWork.LeaveRequest.Add(leaverequest);

            var employee = await _unitOfWork.Employees.GetById(employeeId);
            if (employee == null)
                throw new Exception("Employee not found.");

            Employee? manager = null;
            if (employee.ManagerId.HasValue && employee.ManagerId.Value > 0)
            {
                manager = (await _unitOfWork.Employees.GetAll())
                              .FirstOrDefault(t => t.Id == employee.ManagerId.Value);
            }

            // Fallback: If no direct manager, find HR/Admin contact or self to avoid crashing
            if (manager == null)
            {
                var allEmployees = await _unitOfWork.Employees.GetAll();
                manager = allEmployees.FirstOrDefault(e => e.Id != employeeId && !string.IsNullOrEmpty(e.EmailAddress))
                          ?? employee;
            }

            _unitOfWork.Save();

            // Format dates for email
            string formattedStartDate = GetFormattedDate(leaverequest.StartDate);
            string formattedEndDate = GetFormattedDate(leaverequest.EndDate);
            var managerEmail = manager.EmailAddress ?? employee.EmailAddress ?? string.Empty;
            var managerName = $"{manager.FirstName} {manager.LastName}".Trim();
            var employeeName = $"{employee.FirstName} {employee.LastName}".Trim();
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
                    { "LeaveDescription" ,leaveRequest.LeaveReason ?? string.Empty}
                };

            var subject = $"Leave Request from {employeeName}";

            if (!string.IsNullOrEmpty(managerEmail))
            {
                try
                {
                    await _sendEmailService.SendTemplateEmailAsync(managerEmail, subject, dict, leavetype);
                }
                catch
                {
                    // Ignore email sending failure so leave submission is not blocked
                }
            }

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

        public async Task<IEnumerable<LeaveReqestModel>> GetAllLeaveRequestType()
        {

           var employeeId = _userContext.GetCurrentEmployeeId();


            var requests = await _unitOfWork.LeaveRequest.GetAll();
            var leavetypes = (await _unitOfWork.LeaveType.GetAll());

            var result = from request in requests
                         join leaveType in leavetypes on request.LeaveTypeId equals leaveType.Id
                         where request.EmployeeId == employeeId && request.IsActive == true
                                 && request.IsDeleted != true
                         select new LeaveReqestModel 
                         {
                             Id = request.Id,
                             StartDate = request.StartDate,
                             EndDate = request.EndDate,
                             LeaveMode = request.LeaveMode,
                             LeaveReason = request.LeaveReason,
                             LeaveTypeName = leaveType.Type,
                             LeaveTypeId = leaveType.Id,
                             Status = request.Status,
                             ActionBy = request.ActionBy,
                             ActionDate = request.ActionDate,
                             RejectionReason = request.RejectionReason
                         };


            return result;


        }



        public async Task<LeaveRequest> GetLeaveRequestTypeById(int LeaveRequestTypeId)
        {
            return await _unitOfWork.LeaveRequest.GetByIdAsync(LeaveRequestTypeId);
        }

        public async Task<bool> UpdateLeaveRequestType(LeaveRequest leaveRequest)
        {
            if (leaveRequest == null) return false;

            var existingRequest = await _unitOfWork.LeaveRequest.GetById(leaveRequest.Id);
            if (existingRequest != null && !string.Equals(existingRequest.Status, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                existingRequest.StartDate = leaveRequest.StartDate;
                existingRequest.EndDate = leaveRequest.EndDate;
                existingRequest.UpdatedDate = DateTime.Now;
                existingRequest.IsActive = leaveRequest.IsActive;
                existingRequest.LeaveReason = leaveRequest.LeaveReason;
                existingRequest.LeaveType = leaveRequest.LeaveType;
                existingRequest.LeaveTypeId = leaveRequest.LeaveTypeId;
                existingRequest.LeaveMode = leaveRequest.LeaveMode;

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

       
        public async Task<bool> ApproveLeaveRequestAsync(ApproveLeaveRequest leaveRequest)
        {
            if (leaveRequest == null) return false;

            var leaveRequest1 = await _unitOfWork.LeaveRequest.GetById(leaveRequest.Id);
            if (leaveRequest1 == null) return false;

            var managerId = _userContext.GetCurrentEmployeeId();

            var employee = await _unitOfWork.Employees.GetById(leaveRequest1.EmployeeId);
            var manager = await _unitOfWork.Employees.GetById(managerId);

            var isApprovedDecision = string.Equals(leaveRequest.Status, "Approved", StringComparison.OrdinalIgnoreCase);

            leaveRequest1.Status = isApprovedDecision ? "Approved" : "Rejected";
            leaveRequest1.ActionBy = managerId;
            leaveRequest1.ActionDate = DateTime.Now;
            leaveRequest1.RejectionReason = leaveRequest.RejectionReason;

            _unitOfWork.LeaveRequest.Update(leaveRequest1);
            var result = _unitOfWork.Save();

            if (result <= 0) return false;

            if (employee != null && !string.IsNullOrEmpty(employee.EmailAddress))
            {
                var managerName = manager != null ? $"{manager.FirstName} {manager.LastName}".Trim() : "Manager / HR";
                var managerEmail = manager?.EmailAddress ?? string.Empty;

                var dict = new Dictionary<string, string>
                {
                    { "ManagerName", managerName },
                    { "StartDate", GetFormattedDate(leaveRequest1.StartDate) },
                    { "EndDate", GetFormattedDate(leaveRequest1.EndDate) },
                    { "LeaveReason", isApprovedDecision ? "Approval Request" : (leaveRequest.RejectionReason ?? "Request Rejected") },
                    { "EmployeeName", $"{employee.FirstName} {employee.LastName}".Trim() },
                    { "ManagerEmail", managerEmail }
                };

                try
                {
                    if (isApprovedDecision)
                    {
                        string subject = "Your Leave Request Has Been Approved";
                        await _sendEmailService.SendTemplateEmailAsync(employee.EmailAddress, subject, dict, "ApprovalRequest");
                    }
                    else
                    {
                        string subject = "Your Leave Request Has Been Rejected";
                        await _sendEmailService.SendTemplateEmailAsync(employee.EmailAddress, subject, dict, "RejectRequest");
                    }
                }
                catch
                {
                    // Optional: log the failure but still return success
                }
            }

            return true;
        }

        public async Task<IEnumerable<LeaveRequest>> GetAllEmployeesLeaveRequest()
        {
            try
            {
                var currentUserId = _userContext.GetCurrentEmployeeId();
                var isAdminOrHr = _userContext.IsAdmin() || _userContext.IsHR();

                var leaveRequestsList = await _unitOfWork.LeaveRequest.GetAll();
                var leaveTypesList = await _unitOfWork.LeaveType.GetAll();
                var employeesList = await _unitOfWork.Employees.GetAll();

                var query = from lr in leaveRequestsList
                            join leaveType in leaveTypesList on lr.LeaveTypeId equals leaveType.Id
                            join emp in employeesList on lr.EmployeeId equals emp.Id
                            where lr.IsActive == true && lr.IsDeleted != true
                            select new { lr, leaveType, emp };

                // If not Admin or HR, restrict to reportees of this manager
                if (!isAdminOrHr)
                {
                    query = query.Where(x => x.emp.ManagerId == currentUserId || x.lr.EmployeeId == currentUserId);
                }

                var leaveRequests = query.Select(x => new LeaveRequest
                {
                    Id = x.lr.Id,
                    StartDate = x.lr.StartDate,
                    EndDate = x.lr.EndDate,
                    LeaveMode = x.lr.LeaveMode,
                    LeaveReason = x.lr.LeaveReason,
                    LeaveTypeId = x.lr.LeaveTypeId,
                    Status = x.lr.Status,
                    ActionBy = x.lr.ActionBy,
                    ActionDate = x.lr.ActionDate,
                    RejectionReason = x.lr.RejectionReason,
                    EmployeeId = x.emp.Id,
                    Employee = new Employee
                    {
                        FirstName = x.emp.FirstName,
                        LastName = x.emp.LastName,
                        EmailAddress = x.emp.EmailAddress,
                        ProfileImage = x.emp.ProfileImage,
                        EmployeeCode = x.emp.EmployeeCode
                    },
                    LeaveType = new LeaveType
                    {
                        Type = x.leaveType.Type,
                        Description = x.leaveType.Description,
                        Id = x.leaveType.Id,
                    }
                });

                return leaveRequests.ToList();
            }
            catch
            {
                return new List<LeaveRequest>();
            }
        }

        public async Task<IEnumerable<EmployeeLeaveBalanceDto>> GetEmployeeLeaveBalances(long? targetEmployeeId = null)
        {
            var empId = targetEmployeeId ?? _userContext.GetCurrentEmployeeId();
            var leaveTypes = (await _unitOfWork.LeaveType.GetAll()).Where(t => t.IsActive == true && t.IsDeleted != true).ToList();
            var leaveRequests = (await _unitOfWork.LeaveRequest.GetAll())
                                .Where(lr => lr.EmployeeId == empId && lr.IsActive == true && lr.IsDeleted != true)
                                .ToList();

            var currentYear = DateTime.Now.Year;
            var balances = new List<EmployeeLeaveBalanceDto>();

            foreach (var lt in leaveTypes)
            {
                var typeName = lt.Type ?? "Leave";
                int totalQuota = 14; // Default standard annual leave
                if (typeName.IndexOf("sick", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    totalQuota = 7;
                }
                else if (typeName.IndexOf("casual", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    totalQuota = 8;
                }
                else if (typeName.IndexOf("unpaid", StringComparison.OrdinalIgnoreCase) >= 0 || typeName.IndexOf("loss of pay", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    totalQuota = 0;
                }

                // Filter for requests in current year
                var requestsForType = leaveRequests.Where(r => r.LeaveTypeId == lt.Id && r.StartDate.Year == currentYear).ToList();

                double usedDays = 0;
                int pendingDays = 0;

                foreach (var req in requestsForType)
                {
                    double days = (req.EndDate.Date - req.StartDate.Date).TotalDays + 1;
                    if (days < 0.5) days = 1;

                    if (!string.IsNullOrEmpty(req.LeaveMode) && req.LeaveMode.IndexOf("half", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        days = 0.5;
                    }

                    if (string.Equals(req.Status, "Approved", StringComparison.OrdinalIgnoreCase))
                    {
                        usedDays += days;
                    }
                    else if (string.Equals(req.Status, "Pending", StringComparison.OrdinalIgnoreCase) || (!req.ActionBy.HasValue && !string.Equals(req.Status, "Rejected", StringComparison.OrdinalIgnoreCase)))
                    {
                        pendingDays += (int)Math.Ceiling(days);
                    }
                }

                double remaining = Math.Max(0, totalQuota - usedDays);

                balances.Add(new EmployeeLeaveBalanceDto
                {
                    LeaveTypeId = lt.Id,
                    LeaveTypeName = typeName,
                    TotalQuota = totalQuota,
                    UsedDays = usedDays,
                    RemainingDays = remaining,
                    PendingDays = pendingDays
                });
            }

            return balances;
        }

      
    }
}
