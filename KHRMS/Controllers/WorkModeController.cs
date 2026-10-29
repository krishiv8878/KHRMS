using System.Net;
using KHRMS.Core;
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
    public class WorkModeController : ControllerBase
    {
        private readonly IEmployeeDailyWorkModeService _workModeService;
        private readonly IUserContextService _userContextService;

        public WorkModeController(IEmployeeDailyWorkModeService workModeService, IUserContextService userContextService)
        {
            _workModeService = workModeService;
            _userContextService = userContextService;
        }

        [HttpGet("GetForDate")]
        public async Task<IActionResult> GetForDate([FromQuery] long? employeeId, [FromQuery] string? date)
        {
            try
            {
                long empId = employeeId.HasValue && employeeId.Value > 0 
                    ? employeeId.Value 
                    : _userContextService.GetCurrentEmployeeId();

                DateOnly targetDate = !string.IsNullOrWhiteSpace(date) && DateOnly.TryParse(date, out var parsedDate)
                    ? parsedDate
                    : DateOnly.FromDateTime(DateTime.Today);

                var result = await _workModeService.GetWorkModeForDateAsync(empId, targetDate);

                return Ok(new ApiResponse<WorkModeResponseDTO?>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = result != null ? "Work mode retrieved successfully." : "No explicit work mode record found.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving work mode for date.");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<WorkModeResponseDTO?>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        [HttpGet("GetRange")]
        public async Task<IActionResult> GetRange([FromQuery] long? employeeId, [FromQuery] string? startDate, [FromQuery] string? endDate)
        {
            try
            {
                long empId = employeeId.HasValue && employeeId.Value > 0 
                    ? employeeId.Value 
                    : _userContextService.GetCurrentEmployeeId();

                DateOnly start = !string.IsNullOrWhiteSpace(startDate) && DateOnly.TryParse(startDate, out var pStart)
                    ? pStart
                    : DateOnly.FromDateTime(DateTime.Today.AddDays(-30));

                DateOnly end = !string.IsNullOrWhiteSpace(endDate) && DateOnly.TryParse(endDate, out var pEnd)
                    ? pEnd
                    : DateOnly.FromDateTime(DateTime.Today.AddDays(30));

                var result = await _workModeService.GetWorkModesForRangeAsync(empId, start, end);

                return Ok(new ApiResponse<IEnumerable<WorkModeResponseDTO>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Work modes retrieved successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving work modes for range.");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<IEnumerable<WorkModeResponseDTO>>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message,
                    Data = []
                });
            }
        }

        [HttpGet("GetAllForDate")]
        public async Task<IActionResult> GetAllForDate([FromQuery] string? date)
        {
            try
            {
                DateOnly targetDate = !string.IsNullOrWhiteSpace(date) && DateOnly.TryParse(date, out var parsedDate)
                    ? parsedDate
                    : DateOnly.FromDateTime(DateTime.Today);

                var result = await _workModeService.GetAllWorkModesForDateAsync(targetDate);

                return Ok(new ApiResponse<IEnumerable<WorkModeResponseDTO>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "All work modes for date retrieved successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving all work modes for date.");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<IEnumerable<WorkModeResponseDTO>>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message,
                    Data = []
                });
            }
        }

        [HttpGet("GetAllForRange")]
        public async Task<IActionResult> GetAllForRange([FromQuery] string? startDate, [FromQuery] string? endDate)
        {
            try
            {
                DateOnly start = !string.IsNullOrWhiteSpace(startDate) && DateOnly.TryParse(startDate, out var pStart)
                    ? pStart
                    : DateOnly.FromDateTime(DateTime.Today.AddDays(-30));

                DateOnly end = !string.IsNullOrWhiteSpace(endDate) && DateOnly.TryParse(endDate, out var pEnd)
                    ? pEnd
                    : DateOnly.FromDateTime(DateTime.Today.AddDays(30));

                var result = await _workModeService.GetAllWorkModesForRangeAsync(start, end);

                return Ok(new ApiResponse<IEnumerable<WorkModeResponseDTO>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "All work modes for range retrieved successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving all work modes for range.");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<IEnumerable<WorkModeResponseDTO>>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message,
                    Data = []
                });
            }
        }

        [HttpPost("SetWorkMode")]
        public async Task<IActionResult> SetWorkMode([FromBody] SetWorkModeDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new ApiResponse<WorkModeResponseDTO?>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Invalid work mode request data.",
                    Data = null
                });
            }

            try
            {
                var result = await _workModeService.SetWorkModeAsync(dto, User);

                return Ok(new ApiResponse<WorkModeResponseDTO>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Daily work mode successfully recorded.",
                    Data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new ApiResponse<WorkModeResponseDTO?>
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<WorkModeResponseDTO?>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving work mode.");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<WorkModeResponseDTO?>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message,
                    Data = null
                });
            }
        }
    }
}
