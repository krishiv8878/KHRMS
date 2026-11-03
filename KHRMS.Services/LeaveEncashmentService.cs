using KHRMS.Core;
using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using Serilog;

namespace KHRMS.Services
{
    public class LeaveEncashmentService : ILeaveEncashmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILeaveLedgerService _leaveLedgerService;
        private readonly IUserContextService _userContext;

        public LeaveEncashmentService(
            IUnitOfWork unitOfWork,
            ILeaveLedgerService leaveLedgerService,
            IUserContextService userContext)
        {
            _unitOfWork = unitOfWork;
            _leaveLedgerService = leaveLedgerService;
            _userContext = userContext;
        }

        public async Task<EncashmentEligibilityDto> GetEncashmentEligibilityAsync(long employeeId, long leaveTypeId)
        {
            var empId = employeeId > 0 ? employeeId : _userContext.GetCurrentEmployeeId();
            var lt = await _unitOfWork.LeaveType.GetById(leaveTypeId);
            if (lt == null)
            {
                return new EncashmentEligibilityDto { IsEligible = false, IneligibilityReason = "Leave type not found." };
            }

            if (!lt.EncashmentAllowed)
            {
                return new EncashmentEligibilityDto
                {
                    LeaveTypeId = lt.Id,
                    LeaveTypeName = lt.Type ?? "Leave",
                    EncashmentAllowed = false,
                    IsEligible = false,
                    IneligibilityReason = $"Encashment is not allowed for {lt.Type} under company policy."
                };
            }

            var balance = await _leaveLedgerService.GetBalanceSummaryAsync(empId, leaveTypeId);
            decimal available = (decimal)balance.AvailableDays;

            // Daily salary basis
            var salaryAssignment = (await _unitOfWork.EmployeeSalaryAssignments.GetAll())
                .FirstOrDefault(s => s.EmployeeId == empId && s.IsActive && !s.IsDeleted);

            decimal monthlySalary = salaryAssignment?.MonthlyGross ?? 30000m;
            if (lt.EncashmentSalaryFormula?.IndexOf("Basic", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                monthlySalary = salaryAssignment?.MonthlyGross * 0.5m ?? 15000m;
            }
            decimal dailyRate = Math.Round(monthlySalary / 30m, 2);

            bool isEligible = available >= lt.MinBalanceForEncashment && available > 0;
            string reason = isEligible ? "" : $"Available balance ({available:F1}) is below minimum required balance ({lt.MinBalanceForEncashment}) for encashment.";

            decimal maxDays = Math.Min(lt.MaxEncashmentDaysPerRequest, Math.Max(0, available));

            return new EncashmentEligibilityDto
            {
                LeaveTypeId = lt.Id,
                LeaveTypeName = lt.Type ?? "Leave",
                EncashmentAllowed = true,
                AvailableBalance = available,
                MinBalanceRequired = lt.MinBalanceForEncashment,
                MaxEncashmentDays = maxDays,
                DailySalaryBasis = dailyRate,
                IsEligible = isEligible,
                IneligibilityReason = reason
            };
        }

        public async Task<LeaveEncashmentRequest> ApplyEncashmentAsync(LeaveEncashmentRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            long currentUserId = _userContext.GetCurrentEmployeeId();
            bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR();
            long employeeId = (isPrivileged && request.EmployeeId > 0) ? request.EmployeeId : currentUserId;

            var eligibility = await GetEncashmentEligibilityAsync(employeeId, request.LeaveTypeId);
            if (!eligibility.IsEligible)
            {
                throw new InvalidOperationException(eligibility.IneligibilityReason);
            }

            if (request.RequestedDays <= 0)
            {
                throw new InvalidOperationException("Requested encashment days must be greater than zero.");
            }

            if (request.RequestedDays > eligibility.MaxEncashmentDays)
            {
                throw new InvalidOperationException(
                    $"Requested encashment days ({request.RequestedDays}) exceeds the maximum allowed limit ({eligibility.MaxEncashmentDays} days).");
            }

            request.EmployeeId = employeeId;
            request.DailySalaryBasis = eligibility.DailySalaryBasis;
            request.EstimatedAmount = Math.Round(request.RequestedDays * eligibility.DailySalaryBasis, 2);
            request.Status = "Pending";
            request.CreatedDate = DateTime.UtcNow;
            request.IsActive = true;
            request.IsDeleted = false;

            await _unitOfWork.LeaveEncashmentRequests.Add(request);
            _unitOfWork.Save();

            try
            {
                await _unitOfWork.Notifications.Add(new Notification
                {
                    EmployeeId = employeeId,
                    Title = "Leave Encashment Request Submitted",
                    Message = $"Your request to encash {request.RequestedDays} day(s) of {eligibility.LeaveTypeName} (Est. payout: ₹{request.EstimatedAmount:N2}) was submitted for approval.",
                    Category = "Leave",
                    Type = "info",
                    Icon = "payments",
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

        public async Task<IEnumerable<LeaveEncashmentRequest>> GetAllEncashmentRequestsAsync()
        {
            var list = (await _unitOfWork.LeaveEncashmentRequests.GetAll())
                .Where(r => r.IsDeleted != true)
                .OrderByDescending(r => r.CreatedDate)
                .ToList();

            var emps = (await _unitOfWork.Employees.GetAll()).ToDictionary(e => e.Id);
            var leaveTypes = (await _unitOfWork.LeaveType.GetAll()).ToDictionary(t => t.Id);

            foreach (var req in list)
            {
                if (emps.TryGetValue(req.EmployeeId, out var e)) req.Employee = e;
                if (leaveTypes.TryGetValue(req.LeaveTypeId, out var lt)) req.LeaveType = lt;
            }

            return list;
        }

        public async Task<bool> ApproveEncashmentAsync(long requestId, long approverId)
        {
            var req = await _unitOfWork.LeaveEncashmentRequests.GetById(requestId);
            if (req == null || req.IsDeleted == true) throw new InvalidOperationException("Encashment request not found.");

            if (!string.Equals(req.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Request is already {req.Status}.");
            }

            var balance = await _leaveLedgerService.GetBalanceSummaryAsync(req.EmployeeId, req.LeaveTypeId);
            if ((decimal)balance.AvailableDays < req.RequestedDays)
            {
                throw new InvalidOperationException(
                    $"Cannot approve: Insufficient available balance. Available: {balance.AvailableDays:F1}, Requested: {req.RequestedDays:F1}.");
            }

            // Deduct days from ledger
            var lt = await _unitOfWork.LeaveType.GetById(req.LeaveTypeId);
            await _leaveLedgerService.RecordTransactionAsync(
                req.EmployeeId,
                req.LeaveTypeId,
                "Encashment",
                -req.RequestedDays,
                null,
                $"{DateTime.UtcNow.Year}-{DateTime.UtcNow.Month:D2}",
                $"Leave encashment of {req.RequestedDays} days approved (Est. payout: ₹{req.EstimatedAmount:N2})",
                approverId);

            req.Status = "Approved";
            req.ActionBy = approverId;
            req.ActionDate = DateTime.UtcNow;
            req.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.LeaveEncashmentRequests.Update(req);
            _unitOfWork.Save();

            try
            {
                await _unitOfWork.Notifications.Add(new Notification
                {
                    EmployeeId = req.EmployeeId,
                    Title = "Leave Encashment Approved",
                    Message = $"Your request to encash {req.RequestedDays} day(s) of {lt?.Type} has been approved. Payout of ₹{req.EstimatedAmount:N2} will be processed in the next payroll cycle.",
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

        public async Task<bool> RejectEncashmentAsync(long requestId, long approverId, string rejectionReason)
        {
            var req = await _unitOfWork.LeaveEncashmentRequests.GetById(requestId);
            if (req == null || req.IsDeleted == true) throw new InvalidOperationException("Encashment request not found.");

            req.Status = "Rejected";
            req.ActionBy = approverId;
            req.ActionDate = DateTime.UtcNow;
            req.RejectionReason = rejectionReason;
            req.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.LeaveEncashmentRequests.Update(req);
            _unitOfWork.Save();

            try
            {
                await _unitOfWork.Notifications.Add(new Notification
                {
                    EmployeeId = req.EmployeeId,
                    Title = "Leave Encashment Rejected",
                    Message = $"Your leave encashment request for {req.RequestedDays} day(s) was rejected. Reason: {rejectionReason}",
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
