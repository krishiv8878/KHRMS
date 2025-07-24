using Azure.Core;
using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Net;

namespace KHRMS.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceRequestController(IAttendanceRequestService attendanceRequestService) : ControllerBase
    {
        private readonly IAttendanceRequestService _attendanceRequestService = attendanceRequestService;


        [HttpGet("GetAttendanceRequests")]
        public async Task<IActionResult> GetAttendanceRequests()
        {
            Log.Information("GetAttendanceRequests API called.");

            var attendanceRequests = await _attendanceRequestService.GetAllAsync();
            if (attendanceRequests == null || !attendanceRequests.Any())
            {
                Log.Information("No AttendanceRequest records found.");
                return Ok(new ApiResponse<List<AttendanceRequest>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.AttendanceRequestsNotFound,
                    Data = null
                });
            }

            Log.Information("AttendanceRequest records found successfully.");
            return Ok(new ApiResponse<List<AttendanceRequest>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AttendanceRequestsFound,
                Data = attendanceRequests.ToList()
            });
        }



        [HttpGet("GetAttendanceRequestById")]
        public async Task<IActionResult> GetAttendanceRequestById(long id)
        {
            Log.Information("GetAttendanceRequestById API called for ID {Id}.", id);

            var attendanceRequest = await _attendanceRequestService.GetByIdAsync(id);
            if (attendanceRequest == null)
            {
                Log.Information("AttendanceRequest with ID {Id} not found.", id);
                return Ok(new ApiResponse<AttendanceRequest>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.AttendanceRequestNotFound,
                    Data = null
                });
            }

            Log.Information("AttendanceRequest with ID {Id} found successfully.", id);
            return Ok(new ApiResponse<AttendanceRequest>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AttendanceRequestFound,
                Data = attendanceRequest
            });
        }



        [HttpPost("AddAttendanceRequest")]
        public async Task<IActionResult> AddAttendanceRequest([FromBody] AttendanceRequest attendanceRequest)
        {
            Log.Information("AddAttendanceRequest API called.");

            if (!ModelState.IsValid)
            {
                Log.Warning("Invalid model state for AddAttendanceRequest.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidData,
                    Data = false
                });
            }

            var employee = await _attendanceRequestService.GetByIdAsync(attendanceRequest.EmployeeId);
            if (employee == null)
            {
                Log.Warning("Employee with ID {EmployeeId} not found.", attendanceRequest.EmployeeId);
                return NotFound(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Employee not found",
                    Data = false
                });
            }

            attendanceRequest.ManagerId = employee.ManagerId;
            await _attendanceRequestService.AddAsync(attendanceRequest, User);

            Log.Information("AttendanceRequest added successfully.");
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AttendanceRequestAdded,
                Data = true
            });
        }


        [HttpPut("UpdateAttendanceRequest")]
        public async Task<IActionResult> UpdateAttendanceRequest([FromBody] AttendanceRequest attendanceRequest)
        {
            Log.Information("UpdateAttendanceRequest API called.");

            if (!ModelState.IsValid)
            {
                Log.Warning("Invalid model state for UpdateAttendanceRequest.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidData,
                    Data = false
                });
            }

            await _attendanceRequestService.UpdateAsync(attendanceRequest);
            Log.Information("AttendanceRequest updated successfully.");
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AttendanceRequestUpdated,
                Data = true
            });
        }



        [HttpDelete("DeleteAttendanceRequest")]
        public async Task<IActionResult> DeleteAttendanceRequest(long id)
        {
            Log.Information("DeleteAttendanceRequest API called for ID {Id}.", id);

            await _attendanceRequestService.DeleteAsync(id);
            Log.Information("AttendanceRequest with ID {Id} deleted successfully.", id);
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AttendanceRequestDeleted,
                Data = true
            });
        }
    }

}
