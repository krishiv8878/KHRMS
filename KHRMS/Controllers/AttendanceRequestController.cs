using Azure.Core;
using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Net;

namespace KHRMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceRequestController(IAttendanceRequestService attendanceRequestService) : ControllerBase
    {
        private readonly IAttendanceRequestService _attendanceRequestService = attendanceRequestService;

        //[HttpGet]
        //[Route("GetAttendanceRequests")]
        //public async Task<IActionResult> GetAttendanceRequests()
        //{
        //    var attendanceRequests = await _attendanceRequestService.GetAllAsync();
        //    if (attendanceRequests == null)
        //    {
        //        return NotFound();
        //    }
        //    var response = new ApiResponse<List<AttendanceRequest>>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = attendanceRequests.Any() ? ApiMessageConstant.AttendanceRequestsFound : ApiMessageConstant.AttendanceRequestsNotFound,
        //        Data = attendanceRequests.ToList()
        //    };
        //    return Ok(response);
        //}

        [HttpGet("GetAttendanceRequests")]
        public async Task<IActionResult> GetAttendanceRequests()
        {
            Log.Information("GetAttendanceRequests API called.");

            var attendanceRequests = await _attendanceRequestService.GetAllAsync();
            if (attendanceRequests == null || !attendanceRequests.Any())
            {
                Log.Information("No AttendanceRequest records found.");
                return NotFound(new ApiResponse<List<AttendanceRequest>>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
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
        //[HttpGet]
        //[Route("GetAttendanceRequestById/{id}")]
        //public async Task<IActionResult> GetAttendanceRequestById(long id)
        //{
        //    var attendanceRequest = await _attendanceRequestService.GetByIdAsync(id);
        //    if (attendanceRequest == null)
        //    {
        //        return NotFound(new ApiResponse<AttendanceRequest>
        //        {
        //            StatusCode = (int)HttpStatusCode.NotFound,
        //            Message = ApiMessageConstant.AttendanceRequestNotFound,
        //            Data = null
        //        });
        //    }
        //    var response = new ApiResponse<AttendanceRequest>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.AttendanceRequestFound,
        //        Data = attendanceRequest
        //    };
        //    return Ok(response);
        //}

        [HttpGet("GetAttendanceRequestById")]
        public async Task<IActionResult> GetAttendanceRequestById(long id)
        {
            Log.Information("GetAttendanceRequestById API called for ID {Id}.", id);

            var attendanceRequest = await _attendanceRequestService.GetByIdAsync(id);
            if (attendanceRequest == null)
            {
                Log.Information("AttendanceRequest with ID {Id} not found.", id);
                return NotFound(new ApiResponse<AttendanceRequest>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
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

        //[HttpPost]
        //[Route("AddAttendanceRequest")]
        //public async Task<IActionResult> AddAttendanceRequest([FromBody] AttendanceRequest attendanceRequest)
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


        //    // ✅ Fetch ManagerId from Employee Table
        //    var employee = await _attendanceRequestService.GetByIdAsync(attendanceRequest.EmployeeId);
        //    if (employee == null)
        //    {
        //        return NotFound(new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.NotFound,
        //            Message = "Employee not found",
        //            Data = false
        //        });
        //    }

        //    attendanceRequest.ManagerId = employee.ManagerId; // Assign ManagerId from Employee
        //    await _attendanceRequestService.AddAsync(attendanceRequest, User);
        //    var response = new ApiResponse<bool>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.AttendanceRequestAdded,
        //        Data = true
        //    };
        //    return Ok(response);

        //}

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

        //[HttpPut]
        //[Route("UpdateAttendanceRequest")]
        //public async Task<IActionResult> UpdateAttendanceRequest([FromBody] AttendanceRequest attendanceRequest)
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
        //    await _attendanceRequestService.UpdateAsync(attendanceRequest);
        //    var response = new ApiResponse<bool>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.AttendanceRequestUpdated,
        //        Data = true
        //    };
        //    return Ok(response);
        //}
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


        //[HttpDelete]
        //[Route("DeleteAttendanceRequest/{id}")]
        //public async Task<IActionResult> DeleteAttendanceRequest(long id)
        //{
        //    await _attendanceRequestService.DeleteAsync(id);
        //    var response = new ApiResponse<bool>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.AttendanceRequestDeleted,
        //        Data = true
        //    };
        //    return Ok(response);
        //}

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
