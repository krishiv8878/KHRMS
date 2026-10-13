using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using MimeKit.Cryptography;
using System.Linq;
using System.Security.Claims;


namespace KHRMS.Services
{
    public class AttendanceRequestService : IAttendanceRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmployeeAttendanceService _employeeAttendanceService;
        private readonly IUserContextService _userContextService;
        private readonly ISendEmailService _emailService;
        private readonly IAttendanceLogService _attendanceLogService;

        public AttendanceRequestService(
            IUnitOfWork unitOfWork, 
            IEmployeeAttendanceService employeeAttendanceService, 
            IUserContextService userContextService, 
            ISendEmailService emailService,
            IAttendanceLogService attendanceLogService)
        {
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
            _employeeAttendanceService = employeeAttendanceService;
            _emailService = emailService;
            _attendanceLogService = attendanceLogService;
        }

        public async Task<IEnumerable<AttendanceRequestUpdateDTO>> GetAllAsync()
        {
            long currentEmpId = _userContextService.GetCurrentEmployeeId();
            bool isAdminOrHr = _userContextService.IsAdmin() || _userContextService.IsHR();
            var attendance = await _unitOfWork.AttendanceRequests.GetAll();
            var employees = await _unitOfWork.Employees.GetAll();

            var filteredAttendance = (isAdminOrHr || currentEmpId == 0)
                ? attendance 
                : attendance.Where(x => x.ManagerId == currentEmpId || x.EmployeeId == currentEmpId || x.RequestedBy == currentEmpId);

            var attendanceDtos = (from a in filteredAttendance
                                  join e in employees on a.EmployeeId equals e.Id into empGroup
                                  from e in empGroup.DefaultIfEmpty()
                                  select new AttendanceRequestUpdateDTO
                                  {
                                      Id = a.Id,
                                      EmployeeId = a.EmployeeId,
                                      EmployeeName = e != null ? $"{e.FirstName} {e.LastName}".Trim() : "Employee",
                                      RequestType = a.RequestType ?? "Regularization",
                                      RequestedDate = a.RequestedDate,
                                      RequestedBy = a.RequestedBy,
                                      Reason = a.Reason,
                                      Status = a.Status ?? "Pending",
                                      LastActionBy = a.LastActionBy ?? 0,
                                      ActionBy = a.ActionBy ?? a.LastActionBy,
                                      ActionDate = a.ActionDate,
                                      RejectionReason = a.RejectionReason,
                                      clockIn = a.ClockInTime,
                                      clockOut = a.ClockOutTime ?? DateTime.MinValue,
                                      ManagerId = a.ManagerId,
                                      CreatedBy = a.CreatedBy,
                                      CreatedDate = a.CreatedDate,
                                      UpdatedBy = a.UpdatedBy,
                                      UpdatedDate = a.UpdatedDate,
                                      IsActive = a.IsActive,
                                      IsDeleted = a.IsDeleted
                                  })
                                 .ToList();

            return attendanceDtos;
        }

        public async Task<AttendanceRequest> GetByIdAsync(long id)
        {
            var attendanceRequests = await _unitOfWork.AttendanceRequests.GetById(id);
            return attendanceRequests;
        }

        public async Task AddAsync(AttendanceRequestDTO attendanceRequest, ClaimsPrincipal user)
        {
            if (attendanceRequest == null)
                throw new ArgumentNullException(nameof(attendanceRequest));

            long employeeId = attendanceRequest.EmployeeId.HasValue && attendanceRequest.EmployeeId.Value > 0
                ? attendanceRequest.EmployeeId.Value
                : _userContextService.GetCurrentEmployeeId();

            if (employeeId == 0 && user != null)
            {
                var idStr = user.FindFirst("UserId")?.Value 
                    ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? user.FindFirst("id")?.Value;
                if (long.TryParse(idStr, out var parsedId))
                {
                    employeeId = parsedId;
                }
            }

            var employee = await _unitOfWork.Employees.GetById(employeeId);
            if (employee == null)
                throw new Exception($"Employee with ID {employeeId} not found.");
            
            long targetManagerId = (attendanceRequest.ManagerId.HasValue && attendanceRequest.ManagerId.Value > 0)
                ? attendanceRequest.ManagerId.Value
                : (employee.ManagerId ?? 0);

            if (targetManagerId == 0)
            {
                var adminEmp = (await _unitOfWork.Employees.GetAll()).FirstOrDefault(e => e.Id != employee.Id);
                targetManagerId = adminEmp?.Id ?? employee.Id;
            }

            // Check if there is already an active (Pending or Approved) request for this date
            var existingRequests = (await _unitOfWork.AttendanceRequests.GetAll())
                .Where(x => x.EmployeeId == employee.Id && x.RequestedDate.Date == attendanceRequest.RequestedDate.Date && !x.IsDeleted)
                .ToList();

            var activeRequest = existingRequests.FirstOrDefault(x => 
                string.Equals(x.Status, "Pending", StringComparison.OrdinalIgnoreCase) || 
                string.Equals(x.Status, "Approved", StringComparison.OrdinalIgnoreCase));

            if (activeRequest != null)
            {
                string statusDesc = string.Equals(activeRequest.Status, "Approved", StringComparison.OrdinalIgnoreCase) 
                    ? "already approved" 
                    : "currently pending approval";
                throw new InvalidOperationException($"A regularization request for {attendanceRequest.RequestedDate:yyyy-MM-dd} is {statusDesc}. You can only submit a new request if the previous one was rejected.");
            }

            if (employee.DateOfJoining.HasValue && attendanceRequest.RequestedDate.Date < employee.DateOfJoining.Value.Date)
            {
                throw new InvalidOperationException($"Cannot submit attendance regularization for {attendanceRequest.RequestedDate:yyyy-MM-dd} prior to your official joining date ({employee.DateOfJoining.Value:yyyy-MM-dd}).");
            }

            var request = new AttendanceRequest
            {
                EmployeeId = employee.Id,
                RequestType = attendanceRequest.RequestType ?? "Regularization",
                RequestedDate = attendanceRequest.RequestedDate,
                RequestedBy = employee.Id,
                Reason = attendanceRequest.Reason,
                Status = "Pending",
                LastActionBy = employee.Id,
                ActionBy = null,
                ActionDate = null,
                RejectionReason = null,
                ClockInTime = attendanceRequest.clockIn,
                ClockOutTime = attendanceRequest.clockOut,
                CreatedBy = (int)employee.Id,
                CreatedDate = DateTime.Now,
                ManagerId = targetManagerId,
                IsActive = true,
                IsDeleted = false
            };

            await _unitOfWork.AttendanceRequests.Add(request);
            _unitOfWork.Save();

            try
            {
                var empFullName = $"{employee.FirstName} {employee.LastName}".Trim();
                await _unitOfWork.Notifications.Add(new Notification
                {
                    EmployeeId = 0,
                    Title = "Attendance Adjustment Request",
                    Message = $"{empFullName} requested {request.RequestType} for {request.RequestedDate:yyyy-MM-dd}. Reason: {request.Reason ?? "Regularization"}",
                    Category = "Attendance",
                    Type = "request",
                    Icon = "schedule",
                    IconBg = "#fef3c7",
                    IconColor = "#d97706",
                    Route = "/index/request",
                    QueryParams = "tab=attendance",
                    IsRead = false,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                });
                _unitOfWork.Save();
            }
            catch
            {
            }
        }

        public async Task UpdateAsync(AttendanceRequestUpdateDTO attendanceRequest)
        {
            var request = await _unitOfWork.AttendanceRequests.GetById(attendanceRequest.Id);
            if (request == null)
            {
                throw new KeyNotFoundException($"Attendance request with ID {attendanceRequest.Id} not found.");
            }

            long currentEmpId = _userContextService.GetCurrentEmployeeId();
            long fallbackId = currentEmpId > 0
                ? currentEmpId
                : ((attendanceRequest.LastActionBy.HasValue && attendanceRequest.LastActionBy.Value > 0) ? attendanceRequest.LastActionBy.Value : request.ManagerId);
            long actionById = attendanceRequest.ActionBy ?? fallbackId;

            request.Reason = attendanceRequest.Reason ?? request.Reason;
            request.Status = attendanceRequest.Status;
            request.LastActionBy = actionById;
            request.ActionBy = actionById;
            request.ActionDate = attendanceRequest.ActionDate ?? DateTime.Now;
            request.RejectionReason = attendanceRequest.RejectionReason;
            request.UpdatedBy = (int)actionById;
            request.UpdatedDate = DateTime.Now;

            _unitOfWork.AttendanceRequests.Update(request);
            _unitOfWork.Save();

            var requestedDateOnly = DateOnly.FromDateTime(request.RequestedDate);

            if (string.Equals(attendanceRequest.Status, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                DateTime inTime = request.ClockInTime != default ? request.ClockInTime : attendanceRequest.clockIn;
                DateTime outTime = request.ClockOutTime ?? (attendanceRequest.clockOut.HasValue && attendanceRequest.clockOut.Value != DateTime.MinValue ? attendanceRequest.clockOut.Value : inTime);

                decimal duration = (decimal)(outTime - inTime).TotalHours;
                if (duration <= 0)
                {
                    duration = 8.5m;
                    if (outTime <= inTime)
                    {
                        outTime = inTime.AddHours(9);
                    }
                }

                var existingAttendance = (await _unitOfWork.EmployeeAttendance.GetAll())
                    .FirstOrDefault(r => r.EmployeeId == request.EmployeeId && r.AttendanceDate == requestedDateOnly);

                if (existingAttendance != null)
                {
                    existingAttendance.ClockIn = inTime;
                    existingAttendance.ClockOut = outTime;
                    existingAttendance.TotalHours = duration;
                    existingAttendance.EffectiveHours = duration;
                    existingAttendance.UpdatedDate = DateTime.Now;
                    existingAttendance.UpdatedBy = (int)actionById;
                    _unitOfWork.EmployeeAttendance.Update(existingAttendance);
                }
                else
                {
                    var emp = new EmployeeAttendance
                    {
                        EmployeeId = request.EmployeeId,
                        ClockIn = inTime,
                        ClockOut = outTime,
                        TotalHours = duration,
                        EffectiveHours = duration,
                        AttendanceDate = requestedDateOnly,
                        CreatedDate = DateTime.Now,
                        CreatedBy = (int)actionById,
                        IsActive = true,
                        IsDeleted = false
                    };
                    await _unitOfWork.EmployeeAttendance.Add(emp);
                }

                var attendanceLog = new AttendanceLog
                {
                    EmployeeId = request.EmployeeId,
                    AttendanceDate = requestedDateOnly,
                    InTime = inTime,
                    OutTime = outTime,
                    Duration = duration
                };
                await _attendanceLogService.UpdateAttendanceLogAsync(attendanceLog);
                _unitOfWork.Save();

                // Send email notification to employee
                try
                {
                    var employee = await _unitOfWork.Employees.GetById(request.EmployeeId);
                    var manager = await _unitOfWork.Employees.GetById(actionById);
                    if (employee != null && !string.IsNullOrEmpty(employee.EmailAddress))
                    {
                        TimeZoneInfo istZone;
                        try
                        {
                            istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                        }
                        catch
                        {
                            istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
                        }

                        DateTime inTimeIst = inTime.Kind == DateTimeKind.Utc 
                            ? TimeZoneInfo.ConvertTimeFromUtc(inTime, istZone) 
                            : TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(inTime, DateTimeKind.Utc), istZone);
                        DateTime outTimeIst = outTime.Kind == DateTimeKind.Utc 
                            ? TimeZoneInfo.ConvertTimeFromUtc(outTime, istZone) 
                            : TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(outTime, DateTimeKind.Utc), istZone);

                        var dict = new Dictionary<string, string>
                        {
                            { "EmployeeName", $"{employee.FirstName} {employee.LastName}" },
                            { "Date", request.RequestedDate.ToString("yyyy-MM-dd") },
                            { "InTime", inTimeIst.ToString("hh:mm tt") },
                            { "OutTime", outTimeIst.ToString("hh:mm tt") },
                            { "ManagerName", manager != null ? $"{manager.FirstName} {manager.LastName}" : "Manager" }
                        };
                        await _emailService.SendTemplateEmailAsync(employee.EmailAddress, "Attendance Regularization Approved", dict, "RegularizationRequestApproved");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending attendance approval email: {ex.Message}");
                }
            }
            else if (string.Equals(attendanceRequest.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                // Send email notification to employee
                try
                {
                    var employee = await _unitOfWork.Employees.GetById(request.EmployeeId);
                    var manager = await _unitOfWork.Employees.GetById(actionById);
                    if (employee != null && !string.IsNullOrEmpty(employee.EmailAddress))
                    {
                        var dict = new Dictionary<string, string>
                        {
                            { "EmployeeName", $"{employee.FirstName} {employee.LastName}" },
                            { "Date", request.RequestedDate.ToString("yyyy-MM-dd") },
                            { "Reason", request.RejectionReason ?? "No specific reason provided." },
                            { "ManagerName", manager != null ? $"{manager.FirstName} {manager.LastName}" : "Manager" }
                        };
                        await _emailService.SendTemplateEmailAsync(employee.EmailAddress, "Attendance Regularization Rejected", dict, "RegularizationRequestRejected");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending attendance rejection email: {ex.Message}");
                }
            }

            try
            {
                bool isApproved = string.Equals(attendanceRequest.Status, "Approved", StringComparison.OrdinalIgnoreCase);
                await _unitOfWork.Notifications.Add(new Notification
                {
                    EmployeeId = request.EmployeeId,
                    Title = isApproved ? "Attendance Request Approved" : $"Attendance Request {attendanceRequest.Status}",
                    Message = isApproved
                        ? $"Your attendance adjustment request for {request.RequestedDate:yyyy-MM-dd} has been approved."
                        : $"Your attendance adjustment request for {request.RequestedDate:yyyy-MM-dd} was rejected. Reason: {attendanceRequest.RejectionReason ?? "No reason provided"}",
                    Category = "Attendance",
                    Type = isApproved ? "approval" : "alert",
                    Icon = isApproved ? "check_circle" : "cancel",
                    IconBg = isApproved ? "#f0fdf4" : "#fef2f2",
                    IconColor = isApproved ? "#16a34a" : "#dc2626",
                    Route = "/index/attendance",
                    IsRead = false,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                });
                _unitOfWork.Save();
            }
            catch
            {
            }
        }

        public async Task DeleteAsync(long id)
        {
            await _unitOfWork.AttendanceRequests.DeleteAsync(id);
            _unitOfWork.Save();
        }
    }
}





