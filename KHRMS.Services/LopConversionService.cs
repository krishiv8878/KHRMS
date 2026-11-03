using KHRMS.Core;
using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using Serilog;

namespace KHRMS.Services
{
    public class LopConversionService : ILopConversionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILeaveLedgerService _leaveLedgerService;
        private readonly IUserContextService _userContext;

        public LopConversionService(
            IUnitOfWork unitOfWork,
            ILeaveLedgerService leaveLedgerService,
            IUserContextService userContext)
        {
            _unitOfWork = unitOfWork;
            _leaveLedgerService = leaveLedgerService;
            _userContext = userContext;
        }

        public async Task<IEnumerable<LeaveRequest>> GetEligibleLopRequestsAsync(long employeeId)
        {
            var empId = employeeId > 0 ? employeeId : _userContext.GetCurrentEmployeeId();
            var allLeaves = await _unitOfWork.LeaveRequest.GetAll();
            var leaveTypes = (await _unitOfWork.LeaveType.GetAll()).ToDictionary(t => t.Id);

            var payRuns = _unitOfWork.PayRuns != null ? await _unitOfWork.PayRuns.GetAll() : null;

            var eligible = allLeaves.Where(l =>
                l.EmployeeId == empId &&
                l.IsDeleted != true &&
                string.Equals(l.Status, "Approved", StringComparison.OrdinalIgnoreCase) &&
                !l.IsConvertedFromLop &&
                (l.IsLop || (leaveTypes.TryGetValue(l.LeaveTypeId, out var lt) && !lt.IsPaid)))
                .ToList();

            // Filter out periods where payroll is already Finalized/Disbursed
            var result = new List<LeaveRequest>();
            foreach (var leave in eligible)
            {
                int m = leave.StartDate.Month;
                int y = leave.StartDate.Year;
                var locked = payRuns?.FirstOrDefault(p =>
                    !p.IsDeleted &&
                    p.Year == y &&
                    p.Month == m &&
                    (string.Equals(p.Status, "Approved", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(p.Status, "Disbursed", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(p.Status, "Finalized", StringComparison.OrdinalIgnoreCase)));

                if (locked == null)
                {
                    if (leaveTypes.TryGetValue(leave.LeaveTypeId, out var ltName))
                    {
                        leave.LeaveType = ltName;
                    }
                    result.Add(leave);
                }
            }

            return result;
        }

        public async Task<LopConversionRequest> ApplyConversionAsync(LopConversionRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            long currentUserId = _userContext.GetCurrentEmployeeId();
            bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR();
            long employeeId = (isPrivileged && request.EmployeeId > 0) ? request.EmployeeId : currentUserId;

            var originalLeave = await _unitOfWork.LeaveRequest.GetById(request.OriginalLeaveRequestId);
            if (originalLeave == null || originalLeave.IsDeleted == true)
            {
                throw new InvalidOperationException("Original leave request not found.");
            }

            if (!string.Equals(originalLeave.Status, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only approved LOP requests can be converted to paid leave.");
            }

            if (originalLeave.IsConvertedFromLop)
            {
                throw new InvalidOperationException("This LOP request has already been converted to paid leave.");
            }

            var targetLt = await _unitOfWork.LeaveType.GetById(request.TargetLeaveTypeId);
            if (targetLt == null || !targetLt.IsPaid)
            {
                throw new InvalidOperationException("Target leave type must be an active paid leave type.");
            }

            // Check available balance of target leave type
            var balance = await _leaveLedgerService.GetBalanceSummaryAsync(employeeId, request.TargetLeaveTypeId);
            decimal availableWithNeg = (decimal)balance.AvailableDays + (targetLt.IsNegativeBalanceAllowed ? targetLt.NegativeBalanceLimit : 0);

            if (availableWithNeg < request.ConversionDays)
            {
                throw new InvalidOperationException(
                    $"Insufficient {targetLt.Type} balance. You have {balance.AvailableDays:F1} days available, but requested conversion is {request.ConversionDays:F1} days.");
            }

            request.EmployeeId = employeeId;
            request.Status = "Pending";
            request.CreatedDate = DateTime.UtcNow;
            request.IsActive = true;
            request.IsDeleted = false;

            await _unitOfWork.LopConversionRequests.Add(request);
            _unitOfWork.Save();

            try
            {
                await _unitOfWork.Notifications.Add(new Notification
                {
                    EmployeeId = employeeId,
                    Title = "LOP Conversion Request Submitted",
                    Message = $"Your request to convert {request.ConversionDays} day(s) of LOP to {targetLt.Type} has been submitted for approval.",
                    Category = "Leave",
                    Type = "info",
                    Icon = "transform",
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

        public async Task<IEnumerable<LopConversionRequest>> GetAllConversionRequestsAsync()
        {
            var list = (await _unitOfWork.LopConversionRequests.GetAll())
                .Where(r => r.IsDeleted != true)
                .OrderByDescending(r => r.CreatedDate)
                .ToList();

            var emps = (await _unitOfWork.Employees.GetAll()).ToDictionary(e => e.Id);
            var leaveTypes = (await _unitOfWork.LeaveType.GetAll()).ToDictionary(t => t.Id);

            foreach (var req in list)
            {
                if (emps.TryGetValue(req.EmployeeId, out var e)) req.Employee = e;
                if (leaveTypes.TryGetValue(req.TargetLeaveTypeId, out var lt)) req.TargetLeaveType = lt;
            }

            return list;
        }

        public async Task<bool> ApproveConversionAsync(long conversionRequestId, long approverId)
        {
            var req = await _unitOfWork.LopConversionRequests.GetById(conversionRequestId);
            if (req == null || req.IsDeleted == true) throw new InvalidOperationException("Conversion request not found.");

            if (!string.Equals(req.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Request is already {req.Status}.");
            }

            var targetLt = await _unitOfWork.LeaveType.GetById(req.TargetLeaveTypeId);
            var balance = await _leaveLedgerService.GetBalanceSummaryAsync(req.EmployeeId, req.TargetLeaveTypeId);
            decimal availableWithNeg = (decimal)balance.AvailableDays + (targetLt?.IsNegativeBalanceAllowed == true ? targetLt.NegativeBalanceLimit : 0);

            if (availableWithNeg < req.ConversionDays)
            {
                throw new InvalidOperationException(
                    $"Cannot approve: Insufficient {targetLt?.Type ?? "Paid Leave"} balance at approval time. Available: {balance.AvailableDays:F1}, Required: {req.ConversionDays:F1}.");
            }

            // Deduct paid leave days from ledger
            await _leaveLedgerService.RecordTransactionAsync(
                req.EmployeeId,
                req.TargetLeaveTypeId,
                "LOPToPaidConversion",
                -req.ConversionDays,
                req.OriginalLeaveRequestId,
                null,
                $"Converted {req.ConversionDays} LOP days to {targetLt?.Type} (Conversion Request #{req.Id})",
                approverId);

            // Update original leave request
            var orig = await _unitOfWork.LeaveRequest.GetById(req.OriginalLeaveRequestId);
            if (orig != null)
            {
                orig.IsConvertedFromLop = true;
                orig.PaidDays = req.ConversionDays;
                orig.LopDays = 0;
                orig.LeaveTypeId = req.TargetLeaveTypeId;
                orig.LeaveReason = $"{orig.LeaveReason} [Converted to {targetLt?.Type ?? "Paid Leave"}]";
                orig.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.LeaveRequest.Update(orig);
            }

            // Update status of conversion request
            req.Status = "Approved";
            req.ActionBy = approverId;
            req.ActionDate = DateTime.UtcNow;
            req.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.LopConversionRequests.Update(req);
            _unitOfWork.Save();

            // Synchronize DailyAttendanceRecords
            var dailyRecs = (await _unitOfWork.DailyAttendanceRecords.GetAll())
                .Where(r => r.EmployeeId == req.EmployeeId && r.LeaveRequestId == req.OriginalLeaveRequestId && r.IsDeleted != true)
                .ToList();

            foreach (var rec in dailyRecs)
            {
                rec.Status = "OnLeave";
                rec.Remarks = $"Approved Paid Leave (Converted from LOP to {targetLt?.Type})";
                rec.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.DailyAttendanceRecords.Update(rec);
            }
            _unitOfWork.Save();

            try
            {
                await _unitOfWork.Notifications.Add(new Notification
                {
                    EmployeeId = req.EmployeeId,
                    Title = "LOP Conversion Approved",
                    Message = $"Your request to convert {req.ConversionDays} day(s) of LOP to {targetLt?.Type} has been approved.",
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

        public async Task<bool> RejectConversionAsync(long conversionRequestId, long approverId, string rejectionReason)
        {
            var req = await _unitOfWork.LopConversionRequests.GetById(conversionRequestId);
            if (req == null || req.IsDeleted == true) throw new InvalidOperationException("Conversion request not found.");

            req.Status = "Rejected";
            req.ActionBy = approverId;
            req.ActionDate = DateTime.UtcNow;
            req.RejectionReason = rejectionReason;
            req.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.LopConversionRequests.Update(req);
            _unitOfWork.Save();

            try
            {
                await _unitOfWork.Notifications.Add(new Notification
                {
                    EmployeeId = req.EmployeeId,
                    Title = "LOP Conversion Rejected",
                    Message = $"Your request to convert {req.ConversionDays} day(s) of LOP was rejected. Reason: {rejectionReason}",
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
