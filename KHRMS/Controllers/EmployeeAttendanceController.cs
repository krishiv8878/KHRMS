using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Serilog;
using Microsoft.AspNetCore.Authorization;
namespace KHRMS
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeAttendanceController(IEmployeeAttendanceService employeeAttendanceService) : ControllerBase

    {
        private readonly IEmployeeAttendanceService _attendanceService = employeeAttendanceService;


        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            Log.Information("EmployeeAttendanceController - GetAll called.");

            var attendances = await _attendanceService.GetAllAsync();

            if (attendances == null || !attendances.Any())
            {
                Log.Warning("EmployeeAttendanceController - No attendance records found.");
                return Ok(new ApiResponse<IEnumerable<EmployeeAttendance>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.AttendanceRequestsNotFound,
                    Data = null
                });
            }

            Log.Information("EmployeeAttendanceController - Attendance records retrieved.");
            return Ok(new ApiResponse<IEnumerable<EmployeeAttendance>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AllAttendanceRequestsFound,
                Data = attendances
            });
        }


        [HttpGet]
        [Route("GetByEmployeeId")]
        public async Task<IActionResult> GetByEmployeeId(long employeeId)
        {
            Log.Information("EmployeeAttendanceController - GetByEmployeeId called for ID: {EmployeeId}", employeeId);

            var attendances = await _attendanceService.GetByEmployeeIdAsync(employeeId);
            if (attendances == null)
            {
                Log.Warning("EmployeeAttendanceController - Attendance not found for ID: {EmployeeId}", employeeId);
                return Ok(new ApiResponse<EmployeeAttendance>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.AttendanceRequestNotFound,
                    Data = null
                });
            }

            Log.Information("EmployeeAttendanceController - Attendance retrieved for ID: {EmployeeId}", employeeId);
            return Ok(new ApiResponse<EmployeeAttendance>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AttendanceRequestFound,
                Data = attendances
            });
        }



        [HttpPost("AddEmployeeAttendanceRequest")]
        public async Task<IActionResult> AddEmployeeAttendanceRequest([FromBody] EmployeeAttendance attendance)
        {
            Log.Information("EmployeeAttendanceController - AddEmployeeAttendanceRequest called.");

            if (!ModelState.IsValid)
            {
                Log.Warning("EmployeeAttendanceController - Invalid model state for Add.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidData,
                    Data = false
                });
            }

            await _attendanceService.AddAsync(attendance);
            Log.Information("EmployeeAttendanceController - Attendance request added successfully.");
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AttendanceRequestAdded,
                Data = true
            });
        }


        [HttpPut("UpdateEmployeeAttendanceRequest")]
        public async Task<IActionResult> UpdateEmployeeAttendanceRequest(EmployeeAttendance attendance)
        {
            Log.Information("EmployeeAttendanceController - UpdateEmployeeAttendanceRequest called.");

            if (!ModelState.IsValid)
            {
                Log.Warning("EmployeeAttendanceController - Invalid model state for Update.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidData,
                    Data = false
                });
            }

            await _attendanceService.UpdateAsync(attendance);
            Log.Information("EmployeeAttendanceController - Attendance request updated.");
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AttendanceRequestUpdated,
                Data = true
            });
        }


        [HttpDelete]
        [Route("DeleteEmployeeAttendanceRequest")]
        public async Task<IActionResult> DeleteEmployeeAttendanceRequest(long id)
        {
            Log.Information("EmployeeAttendanceController - DeleteEmployeeAttendanceRequest called for ID: {Id}", id);

            var attendance = await _attendanceService.GetByIdAsync(id);
            if (attendance == null)
            {
                Log.Warning("EmployeeAttendanceController - Attendance with ID: {Id} not found.", id);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.AttendanceRequestNotFound,
                    Data = false
                });
            }

            await _attendanceService.DeleteAsync(id);
            Log.Information("EmployeeAttendanceController - Attendance with ID: {Id} deleted.", id);
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AttendanceRequestDeleted,
                Data = true
            });
        }



        [HttpPost("AddRegularizationRequest")]
        public async Task<IActionResult> Create([FromBody] EmployeeAttendance attendance)
        {
            Log.Information("EmployeeAttendanceController - AddLeaveRequest called.");

            if (attendance == null)
            {
                Log.Warning("EmployeeAttendanceController - Invalid Employee attendance request object.");
                return BadRequest("Invalid data.");
            }

            var result = await _attendanceService.SendRegularizationRequestEmail(attendance);
            if (result)
            {
                Log.Information("EmployeeAttendanceController - Regularization request added successfully.");
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.RegularizationRequestTypeAdded,
                    Data = true
                });
            }

            Log.Warning("EmployeeAttendanceController - Failed to add Regularization request.");
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.RegularizationRequestNotAdded,
                Data = false
            });
        }


        [HttpPost("ApproveRegularizationRequest")]
        public async Task<IActionResult> ApproveRegularization([FromBody] EmployeeAttendance attendance)
        {
            Log.Information("EmployeeAttendanceController - ApproveRegularizationRequest called.");

            if (attendance == null || attendance.Id == 0)
            {
                Log.Warning("EmployeeAttendanceController - Invalid attendance approval request object.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Invalid attendance record provided.",
                    Data = false
                });
            }

            var result = await _attendanceService.ApproveRegularizationRequestAsync(attendance);
            if (result)
            {
                Log.Information("EmployeeAttendanceController - Regularization request approved successfully.");
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.RegularizationRequestApproved,
                    Data = true
                });
            }

            Log.Warning("EmployeeAttendanceController - Failed to approve regularization request.");
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.RegularizationRequestNotApproved,
                Data = false
            });
        }

    }

}



