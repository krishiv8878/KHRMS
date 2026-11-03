using KHRMS.Authorization;
using KHRMS.Core.Models;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;

namespace KHRMS.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveLedgerController : ControllerBase
    {
        private readonly ILeaveLedgerService _leaveLedgerService;
        private readonly IUserContextService _userContext;

        public LeaveLedgerController(
            ILeaveLedgerService leaveLedgerService,
            IUserContextService userContext)
        {
            _leaveLedgerService = leaveLedgerService;
            _userContext = userContext;
        }

        [HttpGet("GetBalanceSummary/{employeeId?}")]
        public async Task<IActionResult> GetBalanceSummary(long? employeeId = null)
        {
            try
            {
                long currentUserId = _userContext.GetCurrentEmployeeId();
                bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR();
                long targetEmpId = (isPrivileged && employeeId.HasValue && employeeId.Value > 0) ? employeeId.Value : (currentUserId > 0 ? currentUserId : (employeeId ?? 0));

                var balances = await _leaveLedgerService.GetAllEmployeeBalancesAsync(targetEmpId);

                return Ok(new ApiResponse<IEnumerable<EmployeeLeaveBalanceDto>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Leave balances fetched successfully.",
                    Data = balances
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching leave balance summary for Employee {EmpId}", employeeId);
                return StatusCode(500, new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        [HttpGet("GetLedgerHistory/{employeeId?}")]
        public async Task<IActionResult> GetLedgerHistory(long? employeeId = null, [FromQuery] long? leaveTypeId = null, [FromQuery] int? year = null)
        {
            try
            {
                long currentUserId = _userContext.GetCurrentEmployeeId();
                bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR();
                long targetEmpId = (isPrivileged && employeeId.HasValue && employeeId.Value > 0) ? employeeId.Value : (currentUserId > 0 ? currentUserId : (employeeId ?? 0));

                var history = await _leaveLedgerService.GetLedgerHistoryAsync(targetEmpId, leaveTypeId, year);

                return Ok(new ApiResponse<IEnumerable<LeaveLedger>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Leave ledger history fetched successfully.",
                    Data = history
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching leave ledger history for Employee {EmpId}", employeeId);
                return StatusCode(500, new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        [HttpPost("TriggerAccrual")]
        [RequirePermission("LEAVE_POLICY_MANAGE")]
        public async Task<IActionResult> TriggerAccrual([FromQuery] int? year = null, [FromQuery] int? month = null)
        {
            try
            {
                int y = year ?? DateTime.UtcNow.Year;
                int m = month ?? DateTime.UtcNow.Month;

                int processed = await _leaveLedgerService.ProcessMonthlyAccrualAsync(y, m);

                return Ok(new ApiResponse<object>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = $"Leave accrual completed: {processed} credit records processed for period {y}-{m:D2}.",
                    Data = new { ProcessedCount = processed, Year = y, Month = m }
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error triggering leave accrual");
                return BadRequest(new ApiResponse<string>
                {
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        [HttpPost("TriggerRollover")]
        [RequirePermission("LEAVE_POLICY_MANAGE")]
        public async Task<IActionResult> TriggerRollover([FromQuery] int? currentYear = null, [FromQuery] int? targetYear = null)
        {
            try
            {
                int cy = currentYear ?? (DateTime.UtcNow.Year - 1);
                int ty = targetYear ?? DateTime.UtcNow.Year;

                int processed = await _leaveLedgerService.ProcessYearEndRolloverAsync(cy, ty);

                return Ok(new ApiResponse<object>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = $"Year-end rollover completed: {processed} rollover records processed for {cy} -> {ty}.",
                    Data = new { ProcessedCount = processed, CurrentYear = cy, TargetYear = ty }
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error triggering year-end rollover");
                return BadRequest(new ApiResponse<string>
                {
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = null
                });
            }
        }
    }
}
