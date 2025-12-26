using KHRMS.Core;
using KHRMS.Infrastructure;
using System.Net;
using KHRMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using KHRMS.Core.Models;

namespace KHRMS.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceLogController(IAttendanceLogService attendanceLogService) : ControllerBase
    {
        private readonly IAttendanceLogService _attendanceLogService = attendanceLogService;
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            Log.Information("AttendanceLogController - GetAll called.");

            var attendanceLog = await _attendanceLogService.GetAllAttendanceLogAsync();

            if (attendanceLog == null || !attendanceLog.Any())
            {
                Log.Warning("AttendanceLogController - No attendanceLog records found.");
                return Ok(new ApiResponse<IEnumerable<AttendanceLog>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.AttendanceRequestsNotFound,
                    Data = null
                });
            }

            Log.Information("AttendanceLogController - AttendanceLog records retrieved.");
            return Ok(new ApiResponse<IEnumerable<AttendanceLog>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AllAttendanceRequestsFound,
                Data = attendanceLog
            });
        }


        [HttpGet]
        [Route("GetAttendanceLogById")]
        public async Task<IActionResult> GetById(long employeeId)
        {
            Log.Information("AttendanceLogController - GetAttendanceLogById called for ID: {EmployeeId}", employeeId);

            var attendances = await _attendanceLogService.GetAttendanceLogByIdAsync(employeeId);
            if (attendances == null)
            {
                Log.Warning("AttendanceLogController - AttendanceLog not found for ID: {EmployeeId}", employeeId);
                return Ok(new ApiResponse<AttendanceLog>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.AttendanceRequestNotFound,
                    Data = null
                });
            }

            Log.Information("AttendanceLogController - AttendanceLog retrieved for ID: {EmployeeId}", employeeId);
            return Ok(new ApiResponse<AttendanceLog>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AttendanceRequestFound,
                Data = attendances
            });
        }



        [HttpPost("AddAttendanceLog")]
        public async Task<IActionResult> AddAttendanceLog([FromBody] AttendanceLog attendanceLog)
        {
            Log.Information("AttendanceLogController - AddAttendanceLog called.");

            if (!ModelState.IsValid)
            {
                Log.Warning("AttendanceLogController - Invalid model state for Add.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidData,
                    Data = false
                });
            }

            await _attendanceLogService.AddAttendanceLogAsync(attendanceLog);
            Log.Information("AttendanceLogController - AttendanceLog added successfully.");
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AttendanceRequestAdded,
                Data = true
            });
        }


        [HttpPut("UpdateAttendanceLog")]
        public async Task<IActionResult> UpdateAttendanceLog(AttendanceLog attendanceLog)
        {
            Log.Information("AttendanceLogController - UpdateAttendanceLog called.");

            if (!ModelState.IsValid)
            {
                Log.Warning("AttendanceLogController - Invalid model state for Update.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidData,
                    Data = false
                });
            }

            await _attendanceLogService.UpdateAttendanceLogAsync(attendanceLog);
            Log.Information("AttendanceLogController - AttendanceLog updated.");
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AttendanceRequestUpdated,
                Data = true
            });
        }


        [HttpDelete]
        [Route("DeleteAttendanceLog")]
        public async Task<IActionResult> DeleteAttendanceLog(long id)
        {
            Log.Information("AttendanceLogController - DeleteAttendanceLog called for ID: {Id}", id);

            var attendance = await _attendanceLogService.GetAttendanceLogByIdAsync(id);
            if (attendance == null)
            {
                Log.Warning("AttendanceLogController - Attendance with ID: {Id} not found.", id);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.AttendanceRequestNotFound,
                    Data = false
                });
            }

            await _attendanceLogService.DeleteAttendanceLogAsync(id);
            Log.Information("AttendanceLogController - Attendance with ID: {Id} deleted.", id);
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AttendanceRequestDeleted,
                Data = true
            });
        }
    }
}
