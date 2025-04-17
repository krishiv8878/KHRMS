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


        //[HttpGet]
        //[Route("GetLeaveType")]
        //public async Task<IActionResult> GetLeaveType()
        //{
        //    var leaveType = await _leaveTypeService.GetAllLeaveType();
        //    if (leaveType == null)
        //    {
        //        return NotFound();
        //    }
        //    // Use the wrapper class to create a consistent response
        //    var response = new ApiResponse<List<LeaveType>>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = leaveType.Any() ? ApiMessageConstant.LeaveTypeFound : ApiMessageConstant.LeaveTypeNotFound,
        //        Data = leaveType.ToList()
        //    };
        //    return Ok(response);
        //}
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

        //[HttpPost]
        //[Route("AddLeaveType")]
        //public async Task<IActionResult> AddLeaveType(LeaveType leaveType)
        //{
        //    var isLeaveTypeAdded = await _leaveTypeService.AddLeaveType(leaveType);
        //    if (isLeaveTypeAdded)
        //    {
        //        // Use the wrapper class to create a consistent response
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.LeaveTypeAdded,
        //            Data = isLeaveTypeAdded
        //        };
        //        return Ok(response);
        //    }
        //    else
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.LeaveTypeNotAdded,
        //            Data = isLeaveTypeAdded
        //        };
        //        return BadRequest(response);
        //    }
        //}
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
        //[HttpPut]
        //[Route("UpdateLeaveType")]
        //public async Task<IActionResult> UpdateLeaveType(LeaveType leaveType)
        //{
        //    var isLeaveTypeEdited = await _leaveTypeService.UpdateLeaveType(leaveType);
        //    if (isLeaveTypeEdited)
        //    {
        //        // Use the wrapper class to create a consistent response
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.LeaveTypeUpdated,
        //            Data = isLeaveTypeEdited
        //        };
        //        return Ok(response);
        //    }
        //    else
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.DesignationNotUpdated,
        //            Data = isLeaveTypeEdited
        //        };
        //        return BadRequest(response);
        //    }
        //}

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

        //[HttpDelete]
        //[Route("DeleteLeaveType")]
        //public async Task<IActionResult> DeleteLeaveType(long LeaveTypeId)
        //{
        //    var isLeaveTypeDeleted = await _leaveTypeService.DeleteLeaveType(LeaveTypeId);
        //    if (isLeaveTypeDeleted)
        //    {
        //        // Use the wrapper class to create a consistent response
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.LeaveTypeDeleted,
        //            Data = isLeaveTypeDeleted
        //        };
        //        return Ok(response);
        //    }
        //    else
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.DesignationNotDeleted,
        //            Data = isLeaveTypeDeleted
        //        };
        //        return BadRequest(response);
        //    }
        //}

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
