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
    public class CompOffController : ControllerBase
    {
        private readonly ICompOffService _compOffService;
        private readonly IUserContextService _userContext;

        public CompOffController(
            ICompOffService compOffService,
            IUserContextService userContext)
        {
            _compOffService = compOffService;
            _userContext = userContext;
        }

        [HttpGet("GetEligibleDates/{employeeId?}")]
        public async Task<IActionResult> GetEligibleDates(long? employeeId = null)
        {
            try
            {
                long currentUserId = _userContext.GetCurrentEmployeeId();
                bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR();
                long targetEmpId = (isPrivileged && employeeId.HasValue && employeeId.Value > 0) ? employeeId.Value : currentUserId;

                var dates = await _compOffService.GetEligibleAttendanceDatesAsync(targetEmpId);

                return Ok(new ApiResponse<IEnumerable<CompOffEligibleDateDto>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Eligible attendance dates for Comp-Off fetched successfully.",
                    Data = dates
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching eligible attendance dates for Comp-Off");
                return StatusCode(500, new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = "An unexpected error occurred while processing the Comp-Off request. Please try again later.",
                    Data = null
                });
            }
        }

        [HttpPost("Apply")]
        public async Task<IActionResult> Apply([FromBody] CompOffGrantRequest request)
        {
            try
            {
                var created = await _compOffService.ApplyCompOffGrantAsync(request);

                return Ok(new ApiResponse<CompOffGrantRequest>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Comp-Off grant request submitted successfully.",
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
                Log.Error(ex, "Error applying for Comp-Off grant");
                return StatusCode(500, new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = "An unexpected error occurred while processing the Comp-Off request. Please try again later.",
                    Data = null
                });
            }
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await _compOffService.GetAllCompOffGrantRequestsAsync();
                long currentEmpId = _userContext.GetCurrentEmployeeId();
                bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR() || _userContext.IsManager();

                if (!isPrivileged)
                {
                    list = list.Where(r => r.EmployeeId == currentEmpId);
                }

                return Ok(new ApiResponse<IEnumerable<CompOffGrantRequest>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Comp-Off grant requests fetched successfully.",
                    Data = list
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching Comp-Off grant requests");
                return StatusCode(500, new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = "An unexpected error occurred while processing the Comp-Off request. Please try again later.",
                    Data = null
                });
            }
        }

        [HttpPost("Approve")]
        public async Task<IActionResult> Approve([FromBody] CompOffActionDto dto)
        {
            try
            {
                long approverId = _userContext.GetCurrentEmployeeId();
                bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR() || _userContext.IsManager();
                if (!isPrivileged)
                {
                    return Forbid();
                }

                var success = await _compOffService.ApproveCompOffGrantAsync(dto.RequestId, approverId);

                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Comp-Off grant approved and credited to employee's balance.",
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
                Log.Error(ex, "Error approving Comp-Off grant");
                return StatusCode(500, new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = "An unexpected error occurred while processing the Comp-Off request. Please try again later.",
                    Data = null
                });
            }
        }

        [HttpPost("Reject")]
        public async Task<IActionResult> Reject([FromBody] CompOffActionDto dto)
        {
            try
            {
                long approverId = _userContext.GetCurrentEmployeeId();
                bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR() || _userContext.IsManager();
                if (!isPrivileged)
                {
                    return Forbid();
                }

                var success = await _compOffService.RejectCompOffGrantAsync(dto.RequestId, approverId, dto.Reason ?? "Rejected by manager");

                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Comp-Off grant rejected.",
                    Data = success
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error rejecting Comp-Off grant");
                return StatusCode(500, new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = "An unexpected error occurred while processing the Comp-Off request. Please try again later.",
                    Data = null
                });
            }
        }
    }

    public class CompOffActionDto
    {
        public long RequestId { get; set; }
        public string? Reason { get; set; }
    }
}
