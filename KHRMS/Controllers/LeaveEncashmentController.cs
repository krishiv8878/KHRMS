using KHRMS.Authorization;
using KHRMS.Core.Models;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;

namespace KHRMS.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveEncashmentController : ControllerBase
    {
        private readonly ILeaveEncashmentService _leaveEncashmentService;
        private readonly IUserContextService _userContext;

        public LeaveEncashmentController(
            ILeaveEncashmentService leaveEncashmentService,
            IUserContextService userContext)
        {
            _leaveEncashmentService = leaveEncashmentService;
            _userContext = userContext;
        }

        [HttpGet("GetEligibility/{leaveTypeId}")]
        public async Task<IActionResult> GetEligibility(long leaveTypeId, [FromQuery] long? employeeId = null)
        {
            try
            {
                long currentUserId = _userContext.GetCurrentEmployeeId();
                bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR();
                long targetEmpId = (isPrivileged && employeeId.HasValue && employeeId.Value > 0) ? employeeId.Value : currentUserId;

                var eligibility = await _leaveEncashmentService.GetEncashmentEligibilityAsync(targetEmpId, leaveTypeId);

                return Ok(new ApiResponse<EncashmentEligibilityDto>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Encashment eligibility evaluated successfully.",
                    Data = eligibility
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching encashment eligibility");
                return StatusCode(500, new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        [HttpPost("Apply")]
        public async Task<IActionResult> Apply([FromBody] LeaveEncashmentRequest request)
        {
            try
            {
                var created = await _leaveEncashmentService.ApplyEncashmentAsync(request);

                return Ok(new ApiResponse<LeaveEncashmentRequest>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Leave encashment request submitted successfully.",
                    Data = created
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<string>
                {
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error applying for leave encashment");
                return StatusCode(500, new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await _leaveEncashmentService.GetAllEncashmentRequestsAsync();
                long currentEmpId = _userContext.GetCurrentEmployeeId();
                bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR() || _userContext.IsManager();

                if (!isPrivileged)
                {
                    list = list.Where(r => r.EmployeeId == currentEmpId);
                }

                return Ok(new ApiResponse<IEnumerable<LeaveEncashmentRequest>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Leave encashment requests fetched successfully.",
                    Data = list
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching leave encashment requests");
                return StatusCode(500, new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        [HttpPost("Approve")]
        public async Task<IActionResult> Approve([FromBody] EncashmentActionDto dto)
        {
            try
            {
                long approverId = _userContext.GetCurrentEmployeeId();
                bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR();
                if (!isPrivileged)
                {
                    return Forbid();
                }

                var success = await _leaveEncashmentService.ApproveEncashmentAsync(dto.RequestId, approverId);

                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Leave encashment approved successfully. Monetary payout scheduled for next payroll cycle.",
                    Data = success
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<string>
                {
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error approving leave encashment request");
                return StatusCode(500, new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        [HttpPost("Reject")]
        public async Task<IActionResult> Reject([FromBody] EncashmentActionDto dto)
        {
            try
            {
                long approverId = _userContext.GetCurrentEmployeeId();
                bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR();
                if (!isPrivileged)
                {
                    return Forbid();
                }

                var success = await _leaveEncashmentService.RejectEncashmentAsync(dto.RequestId, approverId, dto.Reason ?? "Rejected by HR");

                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Leave encashment rejected.",
                    Data = success
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error rejecting leave encashment request");
                return StatusCode(500, new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = null
                });
            }
        }
    }

    public class EncashmentActionDto
    {
        public long RequestId { get; set; }
        public string? Reason { get; set; }
    }
}
