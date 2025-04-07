using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Serilog;
namespace KHRMS
{

    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeAttendanceController(IEmployeeAttendanceService employeeAttendanceService) : ControllerBase

    {
        private readonly IEmployeeAttendanceService _attendanceService = employeeAttendanceService;



        //[HttpGet("GetAll")]
        //public async Task<IActionResult> GetAll()
        //{
        //    var attendances = await _attendanceService.GetAllAsync();
        //    return Ok(new ApiResponse<IEnumerable<EmployeeAttendance>>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.AllAttendanceRequestsFound,
        //        Data = attendances
        //    });
        //}
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            Log.Information("EmployeeAttendanceController - GetAll called.");

            var attendances = await _attendanceService.GetAllAsync();

            if (attendances == null || !attendances.Any())
            {
                Log.Warning("EmployeeAttendanceController - No attendance records found.");
                return NotFound(new ApiResponse<IEnumerable<EmployeeAttendance>>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
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

        //[HttpGet]
        //[Route("GetByEmployeeId/{employeeId}")]
        //public async Task<IActionResult> GetByEmployeeId(long employeeId)
        //{
        //    var attendances = await _attendanceService.GetByEmployeeIdAsync(employeeId);
        //    if (attendances == null)
        //    {
        //        return NotFound(new ApiResponse<EmployeeAttendance>
        //        {
        //            StatusCode = (int)HttpStatusCode.NotFound,
        //            Message = ApiMessageConstant.AttendanceRequestNotFound,
        //            Data = null
        //        });
        //    }
        //    var response = new ApiResponse<EmployeeAttendance>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.AttendanceRequestFound,
        //        Data = attendances
        //    };

        //    return Ok(response);
        //}
        [HttpGet]
        [Route("GetByEmployeeId/{employeeId}")]
        public async Task<IActionResult> GetByEmployeeId(long employeeId)
        {
            Log.Information("EmployeeAttendanceController - GetByEmployeeId called for ID: {EmployeeId}", employeeId);

            var attendances = await _attendanceService.GetByEmployeeIdAsync(employeeId);
            if (attendances == null)
            {
                Log.Warning("EmployeeAttendanceController - Attendance not found for ID: {EmployeeId}", employeeId);
                return NotFound(new ApiResponse<EmployeeAttendance>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
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

        //[HttpPost]
        //[Route("AddEmployeeAttendanceRequest")]
        //public async Task<IActionResult> AddEmployeeAttendanceRequest([FromBody] EmployeeAttendance attendance)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.InvalidData,
        //            Data = false
        //        });
        //    }
        //    await _attendanceService.AddAsync(attendance);
        //    var response = new ApiResponse<bool>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.AttendanceRequestAdded,
        //        Data = true
        //    };
        //    return Ok(response);
        //}

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

        //[HttpPut]
        //[Route("UpdateEmployeeAttendanceRequest")]
        //public async Task<IActionResult> UpdateEmployeeAttendanceRequest(EmployeeAttendance attendance)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.InvalidData,
        //            Data = false
        //        });
        //    }
        //    await _attendanceService.UpdateAsync(attendance);
        //    var response = new ApiResponse<bool>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.AttendanceRequestUpdated,
        //        Data = true
        //    };
        //    return Ok(response);

        //}
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
        //[HttpDelete]
        //[Route("DeleteEmployeeAttendanceRequest/{id}")]
        //public async Task<IActionResult> DeleteEmployeeAttendanceRequest(long id)
        //{

        //    var document = await _attendanceService.GetByIdAsync(id);
        //    if (document == null)
        //        return NotFound();

        //    await _attendanceService.DeleteAsync(id);
        //    return Ok(new ApiResponse<bool>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.AttendanceRequestDeleted,
        //        Data = true
        //    });

        //}
        [HttpDelete]
        [Route("DeleteEmployeeAttendanceRequest/{id}")]
        public async Task<IActionResult> DeleteEmployeeAttendanceRequest(long id)
        {
            Log.Information("EmployeeAttendanceController - DeleteEmployeeAttendanceRequest called for ID: {Id}", id);

            var attendance = await _attendanceService.GetByIdAsync(id);
            if (attendance == null)
            {
                Log.Warning("EmployeeAttendanceController - Attendance with ID: {Id} not found.", id);
                return NotFound(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
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
    }

}



