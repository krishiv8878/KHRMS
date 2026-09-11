using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
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
    public class TimesheetController : ControllerBase
    {
        private readonly ITimesheetService _timesheetService;
        private readonly IUserContextService _userContextService;
        private readonly ILogger<TimesheetController> _logger;

        public TimesheetController(
            ITimesheetService timesheetService,
            IUserContextService userContextService,
            ILogger<TimesheetController> logger)
        {
            _timesheetService = timesheetService;
            _userContextService = userContextService;
            _logger = logger;
        }

        [HttpGet("GetMyTimesheet")]
        public async Task<IActionResult> GetMyTimesheet([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] long? employeeId = null, [FromQuery] long? timesheetId = null)
        {
            var empId = _userContextService.GetCurrentEmployeeId();
            var isManagerOrAdmin = _userContextService.IsManager() || _userContextService.IsHR() || _userContextService.IsAdmin();
            if (isManagerOrAdmin && employeeId.HasValue && employeeId.Value > 0)
            {
                empId = employeeId.Value;
            }
            else if (empId <= 0 && employeeId.HasValue && employeeId.Value > 0)
            {
                empId = employeeId.Value;
            }

            if (empId <= 0)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Unauthorized user. Valid Employee ID required.",
                    Data = null
                });
            }

            Log.Information("TimesheetController - GetMyTimesheet for Employee: {EmpId}, Start: {Start}, End: {End}", empId, startDate, endDate);
            var result = await _timesheetService.GetPeriodTimesheetAsync(empId, startDate, endDate, timesheetId);

            return Ok(new ApiResponse<TimesheetViewDTO>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Timesheet retrieved successfully.",
                Data = result
            });
        }

        [HttpPost("SaveEntry")]
        public async Task<IActionResult> SaveEntry([FromBody] TimesheetEntryDTO dto)
        {
            if (dto == null || dto.ProjectId <= 0 || dto.Hours <= 0 || string.IsNullOrWhiteSpace(dto.TaskDescription))
            {
                return BadRequest(new ApiResponse<string>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Invalid task entry details. Project, Task Description, and positive hours are required.",
                    Data = null
                });
            }

            var employeeId = _userContextService.GetCurrentEmployeeId();
            if (employeeId <= 0)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Unauthorized user.",
                    Data = null
                });
            }

            Log.Information("TimesheetController - SaveEntry for Employee: {EmpId}, Project: {ProjectId}, Date: {Date}", employeeId, dto.ProjectId, dto.EntryDate);
            var saved = await _timesheetService.SaveEntryAsync(dto, employeeId);

            return Ok(new ApiResponse<TimesheetEntryDTO>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Task entry saved successfully.",
                Data = saved
            });
        }

        [HttpDelete("DeleteEntry/{id}")]
        public async Task<IActionResult> DeleteEntry(long id)
        {
            var employeeId = _userContextService.GetCurrentEmployeeId();
            if (employeeId <= 0)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Unauthorized user.",
                    Data = null
                });
            }

            Log.Information("TimesheetController - DeleteEntry: {Id} for Employee: {EmpId}", id, employeeId);
            var success = await _timesheetService.DeleteEntryAsync(id, employeeId);
            if (!success)
            {
                return NotFound(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Task entry not found or unauthorized to delete.",
                    Data = false
                });
            }

            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Task entry deleted successfully.",
                Data = true
            });
        }

        [HttpPost("SubmitPeriod")]
        public async Task<IActionResult> SubmitPeriod([FromBody] TimesheetSubmitDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new ApiResponse<string>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Invalid submission payload.",
                    Data = null
                });
            }

            var employeeId = _userContextService.GetCurrentEmployeeId();
            if (employeeId <= 0)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Unauthorized user.",
                    Data = null
                });
            }

            Log.Information("TimesheetController - SubmitPeriod for Employee: {EmpId}, Start: {Start}, End: {End}", employeeId, dto.StartDate, dto.EndDate);
            var result = await _timesheetService.SubmitPeriodAsync(dto, employeeId);

            return Ok(new ApiResponse<TimesheetViewDTO>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Timesheet submitted for manager approval successfully.",
                Data = result
            });
        }

        [HttpGet("GetPendingApprovals")]
        public async Task<IActionResult> GetPendingApprovals()
        {
            var currentUserId = _userContextService.GetCurrentEmployeeId();
            var isManagerOrAdmin = _userContextService.IsManager() || _userContextService.IsHR() || _userContextService.IsAdmin();

            if (!isManagerOrAdmin)
            {
                return Forbid();
            }

            var isAdminOrHR = _userContextService.IsAdmin() || _userContextService.IsHR();
            Log.Information("TimesheetController - GetPendingApprovals for Manager: {MgrId}, IsAdminOrHR: {IsAdminOrHR}", currentUserId, isAdminOrHR);
            var list = await _timesheetService.GetPendingApprovalsForManagerAsync(currentUserId, isAdminOrHR);

            return Ok(new ApiResponse<List<TimesheetViewDTO>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Pending timesheets retrieved successfully.",
                Data = list
            });
        }

        [HttpPost("ApproveOrReject")]
        public async Task<IActionResult> ApproveOrReject([FromBody] TimesheetApprovalDTO dto)
        {
            if (dto == null || dto.TimesheetId <= 0 || string.IsNullOrWhiteSpace(dto.Status))
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Invalid approval payload.",
                    Data = false
                });
            }

            var currentUserId = _userContextService.GetCurrentEmployeeId();
            var isManagerOrAdmin = _userContextService.IsManager() || _userContextService.IsHR() || _userContextService.IsAdmin();

            if (!isManagerOrAdmin)
            {
                return Forbid();
            }

            Log.Information("TimesheetController - ApproveOrReject Timesheet: {TsId}, Status: {Status}, Manager: {MgrId}", dto.TimesheetId, dto.Status, currentUserId);
            var success = await _timesheetService.ApproveOrRejectAsync(dto, currentUserId);

            if (!success)
            {
                return NotFound(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Timesheet not found or could not be updated.",
                    Data = false
                });
            }

            var isApproved = dto.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase);
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = isApproved ? "Timesheet approved successfully." : "Timesheet rejected successfully.",
                Data = true
            });
        }
    }
}
