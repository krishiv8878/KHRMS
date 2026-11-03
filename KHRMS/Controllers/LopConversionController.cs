using KHRMS.Authorization;
using KHRMS.Core;
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
    public class LopConversionController : ControllerBase
    {
        private readonly ILopConversionService _lopConversionService;
        private readonly IUserContextService _userContext;

        public LopConversionController(
            ILopConversionService lopConversionService,
            IUserContextService userContext)
        {
            _lopConversionService = lopConversionService;
            _userContext = userContext;
        }

        [HttpGet("GetEligibleLop/{employeeId?}")]
        public async Task<IActionResult> GetEligibleLop(long? employeeId = null)
        {
            try
            {
                long currentUserId = _userContext.GetCurrentEmployeeId();
                bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR();
                long targetEmpId = (isPrivileged && employeeId.HasValue && employeeId.Value > 0) ? employeeId.Value : currentUserId;

                var eligibleLeaves = await _lopConversionService.GetEligibleLopRequestsAsync(targetEmpId);

                return Ok(new ApiResponse<IEnumerable<LeaveRequest>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Eligible LOP requests fetched successfully.",
                    Data = eligibleLeaves
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching eligible LOP requests");
                return BadRequest(new ApiResponse<string>
                {
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        [HttpPost("Apply")]
        public async Task<IActionResult> Apply([FromBody] LopConversionRequest request)
        {
            try
            {
                var created = await _lopConversionService.ApplyConversionAsync(request);

                return Ok(new ApiResponse<LopConversionRequest>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "LOP-to-Paid leave conversion request submitted successfully.",
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
                Log.Error(ex, "Error applying for LOP conversion");
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
                var requests = await _lopConversionService.GetAllConversionRequestsAsync();
                long currentEmpId = _userContext.GetCurrentEmployeeId();
                bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR() || _userContext.IsManager();

                if (!isPrivileged)
                {
                    requests = requests.Where(r => r.EmployeeId == currentEmpId);
                }

                return Ok(new ApiResponse<IEnumerable<LopConversionRequest>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "LOP conversion requests fetched successfully.",
                    Data = requests
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching LOP conversion requests");
                return StatusCode(500, new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        [HttpPost("Approve")]
        public async Task<IActionResult> Approve([FromBody] LopConversionActionDto dto)
        {
            try
            {
                long approverId = _userContext.GetCurrentEmployeeId();
                bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR() || _userContext.IsManager();
                if (!isPrivileged)
                {
                    return Forbid();
                }

                var success = await _lopConversionService.ApproveConversionAsync(dto.RequestId, approverId);

                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "LOP conversion request approved and balance deducted.",
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
                Log.Error(ex, "Error approving LOP conversion request");
                return StatusCode(500, new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        [HttpPost("Reject")]
        public async Task<IActionResult> Reject([FromBody] LopConversionActionDto dto)
        {
            try
            {
                long approverId = _userContext.GetCurrentEmployeeId();
                bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR() || _userContext.IsManager();
                if (!isPrivileged)
                {
                    return Forbid();
                }

                var success = await _lopConversionService.RejectConversionAsync(dto.RequestId, approverId, dto.Reason ?? "Rejected by manager");

                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "LOP conversion request rejected.",
                    Data = success
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error rejecting LOP conversion request");
                return StatusCode(500, new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = null
                });
            }
        }
    }

    public class LopConversionActionDto
    {
        public long RequestId { get; set; }
        public string? Reason { get; set; }
    }
}
