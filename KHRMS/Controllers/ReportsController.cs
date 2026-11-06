using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using KHRMS.Authorization;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace KHRMS.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportsService _reportsService;
        private readonly IUserContextService _userContext;

        public ReportsController(IReportsService reportsService, IUserContextService userContext)
        {
            _reportsService = reportsService ?? throw new ArgumentNullException(nameof(reportsService));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        private bool IsAuthorizedForReports()
        {
            return _userContext.IsAdmin() || _userContext.IsHR() || _userContext.IsManager();
        }

        // ================= 1. ATTENDANCE SUMMARY =================

        [HttpGet("attendance")]
        public async Task<IActionResult> GetAttendanceSummary([FromQuery] int? month, [FromQuery] int? year, [FromQuery] string? department)
        {
            if (!IsAuthorizedForReports()) return Forbid();

            int m = month ?? DateTime.UtcNow.Month;
            int y = year ?? DateTime.UtcNow.Year;

            try
            {
                var report = await _reportsService.GetAttendanceSummaryReportAsync(m, y, department);
                return Ok(new ApiResponse<List<AttendanceReportItemDto>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = $"Attendance summary report for {m}/{y} retrieved successfully.",
                    Data = report
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ReportsController - Error generating attendance report");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<List<AttendanceReportItemDto>>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred while generating the attendance summary report.",
                    Data = null
                });
            }
        }

        [HttpGet("attendance/export")]
        public async Task<IActionResult> ExportAttendanceSummary([FromQuery] int? month, [FromQuery] int? year, [FromQuery] string? department)
        {
            if (!IsAuthorizedForReports()) return Forbid();

            int m = month ?? DateTime.UtcNow.Month;
            int y = year ?? DateTime.UtcNow.Year;

            try
            {
                var csvBytes = await _reportsService.ExportAttendanceSummaryCsvAsync(m, y, department);
                return File(csvBytes, "text/csv", $"Attendance_Summary_{y}_{m:D2}.csv");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ReportsController - Error exporting attendance summary CSV");
                return StatusCode((int)HttpStatusCode.InternalServerError, "Failed to export attendance summary CSV.");
            }
        }

        // ================= 2. LEAVE BALANCE =================

        [HttpGet("leave-balance")]
        public async Task<IActionResult> GetLeaveBalance([FromQuery] string? department, [FromQuery] long? leaveTypeId)
        {
            if (!IsAuthorizedForReports()) return Forbid();

            try
            {
                var report = await _reportsService.GetLeaveBalanceReportAsync(department, leaveTypeId);
                return Ok(new ApiResponse<List<LeaveBalanceReportItemDto>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Leave balance report retrieved successfully.",
                    Data = report
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ReportsController - Error generating leave balance report");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<List<LeaveBalanceReportItemDto>>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred while generating the leave balance report.",
                    Data = null
                });
            }
        }

        [HttpGet("leave-balance/export")]
        public async Task<IActionResult> ExportLeaveBalance([FromQuery] string? department, [FromQuery] long? leaveTypeId)
        {
            if (!IsAuthorizedForReports()) return Forbid();

            try
            {
                var csvBytes = await _reportsService.ExportLeaveBalanceCsvAsync(department, leaveTypeId);
                return File(csvBytes, "text/csv", $"Leave_Balances_{DateTime.UtcNow:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ReportsController - Error exporting leave balance CSV");
                return StatusCode((int)HttpStatusCode.InternalServerError, "Failed to export leave balance CSV.");
            }
        }

        // ================= 3. PAYROLL RECONCILIATION =================

        [HttpGet("payroll-reconciliation")]
        public async Task<IActionResult> GetPayrollReconciliation([FromQuery] long? payRunId, [FromQuery] int? month, [FromQuery] int? year, [FromQuery] string? department)
        {
            if (!IsAuthorizedForReports()) return Forbid();

            try
            {
                var report = await _reportsService.GetPayrollReconciliationReportAsync(payRunId, month, year, department);
                return Ok(new ApiResponse<List<PayrollReconciliationReportItemDto>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Payroll reconciliation report retrieved successfully.",
                    Data = report
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ReportsController - Error generating payroll reconciliation report");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<List<PayrollReconciliationReportItemDto>>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred while generating the payroll reconciliation report.",
                    Data = null
                });
            }
        }

        [HttpGet("payroll-reconciliation/export")]
        public async Task<IActionResult> ExportPayrollReconciliation([FromQuery] long? payRunId, [FromQuery] int? month, [FromQuery] int? year, [FromQuery] string? department)
        {
            if (!IsAuthorizedForReports()) return Forbid();

            try
            {
                var csvBytes = await _reportsService.ExportPayrollReconciliationCsvAsync(payRunId, month, year, department);
                return File(csvBytes, "text/csv", $"Payroll_Reconciliation_{DateTime.UtcNow:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ReportsController - Error exporting payroll reconciliation CSV");
                return StatusCode((int)HttpStatusCode.InternalServerError, "Failed to export payroll reconciliation CSV.");
            }
        }

        // ================= 4. HEADCOUNT DEMOGRAPHICS =================

        [HttpGet("headcount-demographics")]
        public async Task<IActionResult> GetHeadcountDemographics()
        {
            if (!IsAuthorizedForReports()) return Forbid();

            try
            {
                var report = await _reportsService.GetHeadcountDemographicsReportAsync();
                return Ok(new ApiResponse<HeadcountDemographicsReportDto>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Headcount and organizational demographics report retrieved successfully.",
                    Data = report
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ReportsController - Error generating headcount demographics report");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<HeadcountDemographicsReportDto>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred while generating headcount demographics report.",
                    Data = null
                });
            }
        }

        [HttpGet("headcount-demographics/export")]
        public async Task<IActionResult> ExportHeadcountDemographics()
        {
            if (!IsAuthorizedForReports()) return Forbid();

            try
            {
                var csvBytes = await _reportsService.ExportHeadcountDemographicsCsvAsync();
                return File(csvBytes, "text/csv", $"Headcount_Demographics_{DateTime.UtcNow:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ReportsController - Error exporting headcount demographics CSV");
                return StatusCode((int)HttpStatusCode.InternalServerError, "Failed to export headcount demographics CSV.");
            }
        }
    }
}
