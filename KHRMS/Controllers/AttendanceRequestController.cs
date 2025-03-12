using Azure.Core;
using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace KHRMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceRequestController(IAttendanceRequestService attendanceRequestService) : ControllerBase
    {
        private readonly IAttendanceRequestService _attendanceRequestService = attendanceRequestService;

        [HttpGet]
        [Route("GetAttendanceRequests")]
        public async Task<IActionResult> GetAttendanceRequests()
        {
            var attendanceRequests = await _attendanceRequestService.GetAllAsync();
            if (attendanceRequests == null)
            {
                return NotFound();
            }
            var response = new ApiResponse<List<AttendanceRequest>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = attendanceRequests.Any() ? ApiMessageConstant.AttendanceRequestsFound : ApiMessageConstant.AttendanceRequestsNotFound,
                Data = attendanceRequests.ToList()
            };
            return Ok(response);
        }


        [HttpGet]
        [Route("GetAttendanceRequestById/{id}")]
        public async Task<IActionResult> GetAttendanceRequestById(long id)
        {
            var attendanceRequest = await _attendanceRequestService.GetByIdAsync(id);
            if (attendanceRequest == null)
            {
                return NotFound(new ApiResponse<AttendanceRequest>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ApiMessageConstant.AttendanceRequestNotFound,
                    Data = null
                });
            }
            var response = new ApiResponse<AttendanceRequest>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AttendanceRequestFound,
                Data = attendanceRequest
            };
            return Ok(response);
        }


        [HttpPost]
        [Route("AddAttendanceRequest")]
        public async Task<IActionResult> AddAttendanceRequest([FromBody] AttendanceRequest attendanceRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidData,
                    Data = false
                });
            }


            // ✅ Fetch ManagerId from Employee Table
            var employee = await _attendanceRequestService.GetByIdAsync(attendanceRequest.EmployeeId);
            if (employee == null)
            {
                return NotFound(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Employee not found",
                    Data = false
                });
            }

            attendanceRequest.ManagerId = employee.ManagerId; // Assign ManagerId from Employee
            await _attendanceRequestService.AddAsync(attendanceRequest, User);
            var response = new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AttendanceRequestAdded,
                Data = true
            };
            return Ok(response);
            
        }

        [HttpPut]
        [Route("UpdateAttendanceRequest")]
        public async Task<IActionResult> UpdateAttendanceRequest([FromBody] AttendanceRequest attendanceRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidData,
                    Data = false
                });
            }
            await _attendanceRequestService.UpdateAsync(attendanceRequest);
            var response = new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AttendanceRequestUpdated,
                Data = true
            };
            return Ok(response);
        }


        [HttpDelete]
        [Route("DeleteAttendanceRequest/{id}")]
        public async Task<IActionResult> DeleteAttendanceRequest(long id)
        {
            await _attendanceRequestService.DeleteAsync(id);
            var response = new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AttendanceRequestDeleted,
                Data = true
            };
            return Ok(response);
        }
    }

}
