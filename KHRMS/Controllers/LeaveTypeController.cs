using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;

namespace KHRMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveTypeController(ILeaveTypeService leaveTypeService) : ControllerBase
    {
        public readonly ILeaveTypeService _leaveTypeService = leaveTypeService;


        [HttpGet("GetLeaveType")]
        public async Task<IActionResult> GetLeaveType()
        {
            Log.Information("LeaveTypeController - GetLeaveType called.");
            var leaveTypeList = await _leaveTypeService.GetAllLeaveType();

            if (leaveTypeList == null || !leaveTypeList.Any())
            {
                Log.Warning("LeaveTypeController - No leave types found.");
                return NotFound(new ApiResponse<List<LeaveType>>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ApiMessageConstant.LeaveTypeNotFound,
                    Data = null
                });
            }

            Log.Information("LeaveTypeController - {Count} leave types found.", leaveTypeList.Count());
            return Ok(new ApiResponse<List<LeaveType>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.LeaveTypeFound,
                Data = leaveTypeList.ToList()
            });
        }


        [HttpPost("AddLeaveType")]
        public async Task<IActionResult> AddLeaveType([FromBody] LeaveType leaveType)
        {
            Log.Information("LeaveTypeController - AddLeaveType called.");

            if (leaveType == null)
            {
                Log.Warning("LeaveTypeController - Invalid leave type object.");
                return BadRequest("Invalid data.");
            }

            var result = await _leaveTypeService.AddLeaveType(leaveType);

            if (result)
            {
                Log.Information("LeaveTypeController - Leave type added successfully.");
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.LeaveTypeAdded,
                    Data = true
                });
            }

            Log.Warning("LeaveTypeController - Failed to add leave type.");
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.LeaveTypeNotAdded,
                Data = false
            });
        }


        [HttpPut("UpdateLeaveType")]
        public async Task<IActionResult> UpdateLeaveType([FromBody] LeaveType leaveType)
        {
            Log.Information("LeaveTypeController - UpdateLeaveType called for ID: {Id}", leaveType.Id);

            if (leaveType.Id == null)
            {
                Log.Warning("LeaveTypeController - ID Not Found: URL ID {Id}, Body ID {BodyId}",  leaveType.Id);
                return BadRequest("ID Does Not Exist!");
            }

            var result = await _leaveTypeService.UpdateLeaveType(leaveType);

            if (result)
            {
                Log.Information("LeaveTypeController - Leave type updated successfully for ID: {Id}", leaveType.Id);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.LeaveTypeUpdated,
                    Data = true
                });
            }

            Log.Warning("LeaveTypeController - Failed to update leave type for ID: {Id}", leaveType.Id);
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.LeaveTypeNotUpdated,
                Data = false
            });
        }


        [HttpDelete("DeleteLeaveType")]
        public async Task<IActionResult> DeleteLeaveType(long id)
        {
            Log.Information("LeaveTypeController - DeleteLeaveType called for ID: {Id}", id);

            var result = await _leaveTypeService.DeleteLeaveType(id);

            if (result)
            {
                Log.Information("LeaveTypeController - Leave type deleted for ID: {Id}", id);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.LeaveTypeDeleted,
                    Data = true
                });
            }

            Log.Warning("LeaveTypeController - Failed to delete leave type for ID: {Id}", id);
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.LeaveTypeNotDeleted,
                Data = false
            });
        }
    }
}
