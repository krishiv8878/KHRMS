using KHRMS.Core;
using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using Serilog;

namespace KHRMS.Services
{
    public class CompOffService : ICompOffService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILeaveLedgerService _leaveLedgerService;
        private readonly IUserContextService _userContext;

        public CompOffService(
            IUnitOfWork unitOfWork,
            ILeaveLedgerService leaveLedgerService,
            IUserContextService userContext)
        {
            _unitOfWork = unitOfWork;
            _leaveLedgerService = leaveLedgerService;
            _userContext = userContext;
        }

        public async Task<IEnumerable<CompOffEligibleDateDto>> GetEligibleAttendanceDatesAsync(long employeeId)
        {
            var empId = employeeId > 0 ? employeeId : _userContext.GetCurrentEmployeeId();
            var ninetyDaysAgo = DateTime.UtcNow.Date.AddDays(-90);

            var holidays = (await _unitOfWork.Holidays.GetAll())
                .Where(h => h.IsActive != false && h.IsDeleted != true)
                .ToDictionary(h => h.HolidayDate, h => h.Description ?? "Official Holiday");

            var existingRequests = (await _unitOfWork.CompOffGrantRequests.GetAll())
                .Where(r => r.EmployeeId == empId && r.IsDeleted != true)
                .ToDictionary(r => r.AttendanceDate.Date);

            var dailyRecords = (await _unitOfWork.DailyAttendanceRecords.GetAll())
                .Where(r => r.EmployeeId == empId && r.AttendanceDate.ToDateTime(TimeOnly.MinValue) >= ninetyDaysAgo && r.IsDeleted != true)
                .ToList();

            var eligibleList = new List<CompOffEligibleDateDto>();

            foreach (var rec in dailyRecords)
            {
                var dt = rec.AttendanceDate.ToDateTime(TimeOnly.MinValue);
                bool isWeekend = dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday;
                bool isHoliday = holidays.TryGetValue(rec.AttendanceDate, out var holName);

                if (!isWeekend && !isHoliday) continue;

                // Needs at least 4 hours (240 mins) of effective work
                int workedMins = rec.EffectiveWorkMinutes > 0 ? rec.EffectiveWorkMinutes : rec.TotalWorkMinutes;
                if (workedMins < 240) continue;

                decimal eligibleDays = workedMins >= 480 ? 1.0m : 0.5m;
                bool alreadyRequested = existingRequests.TryGetValue(dt.Date, out var req);

                eligibleList.Add(new CompOffEligibleDateDto
                {
                    AttendanceDate = dt,
                    DayType = isHoliday ? "Holiday" : "Weekend",
                    HolidayName = isHoliday ? holName : null,
                    WorkedMinutes = workedMins,
                    EligibleDays = eligibleDays,
                    AlreadyRequested = alreadyRequested,
                    RequestStatus = req?.Status
                });
            }

            return eligibleList.OrderByDescending(x => x.AttendanceDate);
        }

        public async Task<CompOffGrantRequest> ApplyCompOffGrantAsync(CompOffGrantRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            long currentUserId = _userContext.GetCurrentEmployeeId();
            bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR();
            long employeeId = (isPrivileged && request.EmployeeId > 0) ? request.EmployeeId : currentUserId;

            var existing = (await _unitOfWork.CompOffGrantRequests.GetAll())
                .FirstOrDefault(r => r.EmployeeId == employeeId && r.AttendanceDate.Date == request.AttendanceDate.Date &&
                                     r.IsDeleted != true && r.Status != "Rejected");

            if (existing != null)
            {
                throw new InvalidOperationException(
                    $"A Comp-Off credit request has already been submitted for {request.AttendanceDate:yyyy-MM-dd} (Status: {existing.Status}).");
            }

            request.EmployeeId = employeeId;
            request.Status = "Pending";
            request.CreatedDate = DateTime.UtcNow;
            request.IsActive = true;
            request.IsDeleted = false;

            await _unitOfWork.CompOffGrantRequests.Add(request);
            _unitOfWork.Save();

            try
            {
                await _unitOfWork.Notifications.Add(new Notification
                {
                    EmployeeId = employeeId,
                    Title = "Comp-Off Grant Request Submitted",
                    Message = $"Your request for {request.EligibleDays} Comp-Off credit(s) for working on {request.AttendanceDate:yyyy-MM-dd} has been submitted for approval.",
                    Category = "Leave",
                    Type = "info",
                    Icon = "event_available",
                    IconBg = "#eff6ff",
                    IconColor = "#2563eb",
                    Route = "/index/leaveRequest",
                    IsRead = false,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                });
                _unitOfWork.Save();
            }
            catch { }

            return request;
        }

        public async Task<IEnumerable<CompOffGrantRequest>> GetAllCompOffGrantRequestsAsync()
        {
            var list = (await _unitOfWork.CompOffGrantRequests.GetAll())
                .Where(r => r.IsDeleted != true)
                .OrderByDescending(r => r.CreatedDate)
                .ToList();

            var emps = (await _unitOfWork.Employees.GetAll()).ToDictionary(e => e.Id);
            foreach (var req in list)
            {
                if (emps.TryGetValue(req.EmployeeId, out var e)) req.Employee = e;
            }

            return list;
        }

        public async Task<bool> ApproveCompOffGrantAsync(long requestId, long approverId)
        {
            var req = await _unitOfWork.CompOffGrantRequests.GetById(requestId);
            if (req == null || req.IsDeleted == true) throw new InvalidOperationException("Comp-Off grant request not found.");

            if (!string.Equals(req.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Request is already {req.Status}.");
            }

            // Find or get Comp-Off leave type
            var compOffLt = (await _unitOfWork.LeaveType.GetAll())
                .FirstOrDefault(t => (t.IsCompOff || string.Equals(t.Type, "Compensatory Off", StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(t.Type, "Comp-Off", StringComparison.OrdinalIgnoreCase)) && t.IsActive != false && t.IsDeleted != true);

            if (compOffLt == null)
            {
                // Auto create Comp-Off leave type if none exists
                compOffLt = new LeaveType
                {
                    Type = "Compensatory Off",
                    Description = "Earned compensatory time off for working on weekends or official company holidays.",
                    AllowedDays = 0,
                    IsPaid = true,
                    IsCompOff = true,
                    CompOffValidityDays = 90,
                    AccrualEnabled = false,
                    CarryForwardEnabled = false,
                    EncashmentAllowed = false,
                    SandwichRuleEnabled = false,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };
                await _unitOfWork.LeaveType.Add(compOffLt);
                _unitOfWork.Save();
            }

            // Credit days in ledger & entitlement
            await _leaveLedgerService.RecordTransactionAsync(
                req.EmployeeId,
                compOffLt.Id,
                "CompOffCredit",
                req.EligibleDays,
                null,
                $"{DateTime.UtcNow.Year}-{DateTime.UtcNow.Month:D2}",
                $"Comp-Off earned for attendance on {req.AttendanceDate:yyyy-MM-dd} ({req.EligibleDays} day(s), valid for {compOffLt.CompOffValidityDays} days)",
                approverId);

            req.Status = "Approved";
            req.ActionBy = approverId;
            req.ActionDate = DateTime.UtcNow;
            req.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.CompOffGrantRequests.Update(req);
            _unitOfWork.Save();

            try
            {
                await _unitOfWork.Notifications.Add(new Notification
                {
                    EmployeeId = req.EmployeeId,
                    Title = "Comp-Off Grant Approved",
                    Message = $"Your Comp-Off grant of {req.EligibleDays} day(s) for {req.AttendanceDate:yyyy-MM-dd} has been approved and credited to your leave balance.",
                    Category = "Leave",
                    Type = "approval",
                    Icon = "check_circle",
                    IconBg = "#f0fdf4",
                    IconColor = "#16a34a",
                    Route = "/index/leaveRequest",
                    IsRead = false,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                });
                _unitOfWork.Save();
            }
            catch { }

            return true;
        }

        public async Task<bool> RejectCompOffGrantAsync(long requestId, long approverId, string rejectionReason)
        {
            var req = await _unitOfWork.CompOffGrantRequests.GetById(requestId);
            if (req == null || req.IsDeleted == true) throw new InvalidOperationException("Comp-Off grant request not found.");

            req.Status = "Rejected";
            req.ActionBy = approverId;
            req.ActionDate = DateTime.UtcNow;
            req.RejectionReason = rejectionReason;
            req.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.CompOffGrantRequests.Update(req);
            _unitOfWork.Save();

            try
            {
                await _unitOfWork.Notifications.Add(new Notification
                {
                    EmployeeId = req.EmployeeId,
                    Title = "Comp-Off Grant Rejected",
                    Message = $"Your Comp-Off credit request for {req.AttendanceDate:yyyy-MM-dd} was rejected. Reason: {rejectionReason}",
                    Category = "Leave",
                    Type = "alert",
                    Icon = "cancel",
                    IconBg = "#fef2f2",
                    IconColor = "#ef4444",
                    Route = "/index/leaveRequest",
                    IsRead = false,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                });
                _unitOfWork.Save();
            }
            catch { }

            return true;
        }
    }
}
