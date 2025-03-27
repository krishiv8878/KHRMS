using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Infrastructure.Migrations;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;


namespace KHRMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveRequestController(ILeaveRequestTypeService leaveRequestTypeService) : ControllerBase
    {
        public readonly ILeaveRequestTypeService _leaveRequestTypeService = leaveRequestTypeService;


        [HttpGet]
        [Route("GetAllLeaveRequest")]

        public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetAll()
        {
            var result = await _leaveRequestTypeService.GetAllLeaveRequestType();
            if (result == null)
            {
                return NotFound();
        }
            // Use the wrapper class to create a consistent response
            var response = new ApiResponse<List<LeaveRequest>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = result.Any() ? ApiMessageConstant.LeaveRequestTypeFound : ApiMessageConstant.LeaveRequestTypeNotFound,
                Data = result.ToList()
            };
            return Ok(response);
        }

        //[HttpGet("{id:int}")]
        [HttpGet]

        [HttpGet]
        [Route("GetLeaveRequestById/{id}")]
        public async Task<ActionResult<LeaveRequest>> GetById(int id)
        {
            var result = await _leaveRequestTypeService.GetLeaveRequestTypeById(id);

            if (result == null)
            {
                return NotFound(new ApiResponse<LeaveRequest>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ApiMessageConstant.LeaveRequestNotFound,
                    Data = null
                });
        }
            var response = new ApiResponse<LeaveRequest>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.LeaveRequestFound,
                Data = result
            };
            return Ok(response);
        }

        [HttpPost]
        [Route("AddLeaveRequest")]

        public async Task<IActionResult> Create([FromBody] LeaveRequest leaveRequest)
        {
            if (leaveRequest == null)
                return BadRequest("Invalid data.");
               // return CreatedAtAction(nameof(GetById), new { id = leaveRequest.Id }, leaveRequest);


            var isleaverequest = await _leaveRequestTypeService.AddLeaveRequestType(leaveRequest);
            if (isleaverequest)
            {
                // Use the wrapper class to create a consistent response
                var response = new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.LeaveRequestTypeAdded,
                    Data = isleaverequest
                };
                return Ok(response);

            }
            else
            {
                var response = new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.LeaveRequestTypeNotAdded,
                    Data = isleaverequest
                };
                return BadRequest(response);
            }
        }

        [HttpPut]

        [Route("UpdateLeaveRequest/{id}")]

        public async Task<IActionResult> Update(long id, [FromBody] LeaveRequest leaveRequest)
        {
            if (id != leaveRequest.Id)
                return BadRequest("ID mismatch.");

            var isLeaveTypeUpdated = await _leaveRequestTypeService.UpdateLeaveRequestType(leaveRequest);
            if (isLeaveTypeUpdated)
            {
                // Use the wrapper class to create a consistent response
                var response = new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.LeaveRequestDeleted,
                    Data = isLeaveTypeUpdated
                };
                return Ok(response);
            }
            else
            {
                var response = new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.LeaveRequestNotDeleted,
                    Data = isLeaveTypeUpdated
                };
                return BadRequest(response);
            }
        }

        [HttpDelete]
        [Route("DeleteLeaveRequest/{id}")]

        public async Task<IActionResult> Delete(long id)
        {
            var isLeaveRequestDeleted =  await _leaveRequestTypeService.DeleteLeaveRequestType(id);
            if (isLeaveRequestDeleted)
            {
                // Use the wrapper class to create a consistent response
                var response = new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.LeaveRequestDeleted,
                    Data = isLeaveRequestDeleted
                };
                return Ok(response);
            }
            else
            {
                var response = new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.LeaveRequestNotDeleted,
                    Data = isLeaveRequestDeleted
                };
                return BadRequest(response);
            }
        }
    }
}

   