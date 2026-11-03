using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using KHRMS.Authorization;
using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using KHRMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace KHRMS
{
    public class TriggerComputationRequest
    {
        public string? TargetDate { get; set; } // "yyyy-MM-dd"
        public string? FromDate { get; set; }   // "yyyy-MM-dd"
        public string? ToDate { get; set; }     // "yyyy-MM-dd"
        public long? EmployeeId { get; set; }
    }

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DailyAttendanceController : ControllerBase
    {
        private readonly IDailyAttendanceService _dailyAttendanceService;
        private readonly IUserContextService _userContext;

        public DailyAttendanceController(IDailyAttendanceService dailyAttendanceService, IUserContextService userContext)
        {
            _dailyAttendanceService = dailyAttendanceService;
            _userContext = userContext;
        }

        [HttpGet("GetDailyRecords")]
        [RequirePermission("ATTENDANCE_VIEW_SELF")]
        public async Task<IActionResult> GetDailyRecords([FromQuery] long? employeeId, [FromQuery] int month, [FromQuery] int year)
        {
            Log.Information("DailyAttendanceController - GetDailyRecords called for Employee={EmployeeId}, {Month}/{Year}", employeeId, month, year);

            bool canViewAll = await _userContext.HasPermissionAsync("ATTENDANCE_VIEW_ALL");
            long currentEmpId = _userContext.GetCurrentEmployeeId();

            long? effectiveEmpId = employeeId;
            if (!canViewAll)
            {
                effectiveEmpId = currentEmpId;
            }
            else if (!employeeId.HasValue || employeeId.Value <= 0)
            {
                effectiveEmpId = null; // View all
            }

            if (month < 1 || month > 12) month = DateTime.Today.Month;
            if (year < 2000 || year > 2100) year = DateTime.Today.Year;

            var records = await _dailyAttendanceService.GetDailyRecordsAsync(effectiveEmpId, month, year);

            return Ok(new ApiResponse<IEnumerable<DailyAttendanceRecordDTO>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Daily computed attendance records retrieved.",
                Data = records
            });
        }

        [HttpGet("GetMonthlySummary")]
        [RequirePermission("ATTENDANCE_VIEW_SELF")]
        public async Task<IActionResult> GetMonthlySummary([FromQuery] long? employeeId, [FromQuery] int month, [FromQuery] int year)
        {
            Log.Information("DailyAttendanceController - GetMonthlySummary called for Employee={EmployeeId}, {Month}/{Year}", employeeId, month, year);

            bool canViewAll = await _userContext.HasPermissionAsync("ATTENDANCE_VIEW_ALL");
            long currentEmpId = _userContext.GetCurrentEmployeeId();

            long targetEmpId = (canViewAll && employeeId.HasValue && employeeId.Value > 0) ? employeeId.Value : currentEmpId;

            if (targetEmpId <= 0)
            {
                return BadRequest(new ApiResponse<string>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Valid employee ID is required.",
                    Data = null
                });
            }

            if (month < 1 || month > 12) month = DateTime.Today.Month;
            if (year < 2000 || year > 2100) year = DateTime.Today.Year;

            var summary = await _dailyAttendanceService.GetMonthlySummaryAsync(targetEmpId, month, year);

            return Ok(new ApiResponse<AttendanceMonthlySummaryDTO>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Attendance monthly summary computed successfully.",
                Data = summary
            });
        }

        [HttpPost("TriggerDailyComputation")]
        [RequirePermission("ATTENDANCE_VIEW_ALL")]
        public async Task<IActionResult> TriggerDailyComputation([FromBody] TriggerComputationRequest request)
        {
            Log.Information("DailyAttendanceController - TriggerDailyComputation requested.");

            try
            {
                int count = 0;
                if (!string.IsNullOrWhiteSpace(request.FromDate) && !string.IsNullOrWhiteSpace(request.ToDate))
                {
                    if (DateOnly.TryParse(request.FromDate, out var fromD) && DateOnly.TryParse(request.ToDate, out var toD))
                    {
                        count = await _dailyAttendanceService.ComputeDateRangeAsync(fromD, toD, request.EmployeeId);
                    }
                    else
                    {
                        return BadRequest(new ApiResponse<string>
                        {
                            StatusCode = (int)HttpStatusCode.BadRequest,
                            Message = "Invalid date format. Use yyyy-MM-dd.",
                            Data = null
                        });
                    }
                }
                else
                {
                    var targetDate = !string.IsNullOrWhiteSpace(request.TargetDate) && DateOnly.TryParse(request.TargetDate, out var td)
                        ? td
                        : DateOnly.FromDateTime(DateTime.Today);

                    count = await _dailyAttendanceService.ComputeDailyAttendanceAsync(targetDate, request.EmployeeId);
                }

                return Ok(new ApiResponse<int>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = $"Successfully computed daily attendance records. Processed count: {count}",
                    Data = count
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to manually trigger attendance computation.");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<string>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message,
                    Data = null
                });
            }
        }
    }
}
