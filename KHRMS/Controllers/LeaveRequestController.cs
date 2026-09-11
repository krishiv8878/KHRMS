using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Infrastructure;
using KHRMS.Infrastructure.Migrations;
using KHRMS.Services;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;


namespace KHRMS
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveRequestController(ILeaveRequestTypeService leaveRequestTypeService, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        public readonly ILeaveRequestTypeService _leaveRequestTypeService = leaveRequestTypeService;
        private readonly IUserContextService _userContext; public readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        [HttpGet("GetAllLeaveRequest")]
        public async Task<IActionResult> GetAll()
        {
            
            Log.Information("LeaveRequestController - GetAllLeaveRequest called.");
            var result = await _leaveRequestTypeService.GetAllLeaveRequestType();

            if (result == null || !result.Any())
            {
                Log.Warning("LeaveRequestController - No leave requests found.");
                return Ok(new ApiResponse<List<LeaveRequest>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.LeaveRequestTypeNotFound,
                    Data = null
                });
            }

            Log.Information("LeaveRequestController - {Count} leave requests found.", result.Count());
            return Ok(new ApiResponse<List<LeaveReqestModel>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.LeaveRequestTypeFound,
                Data = result.ToList()
            });
        }


        [HttpGet("GetAllEmployeesLeaveRequest")]
        [Authorize(Roles = "Admin,System Admin,HR,HR Operations,Manager,Management")]
        public async Task<IActionResult> GetAllEmployeesLeaveRequest()
        {
            var result = await _leaveRequestTypeService.GetAllEmployeesLeaveRequest( );

            if (result == null || !result.Any())
            {
                Log.Warning("LeaveRequestController - No leave requests found.");
                return Ok(new ApiResponse<IEnumerable<LeaveRequest>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.LeaveRequestTypeNotFound,
                    Data = null
                });
            }

            Log.Information("LeaveRequestController - {Count} leave requests found.", result.Count());
            return Ok(new ApiResponse<IEnumerable<LeaveRequest>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.LeaveRequestTypeFound,
                Data = result
            });
        }


        [HttpGet("GetLeaveRequestById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            Log.Information("LeaveRequestController - GetLeaveRequestById called with ID: {Id}", id);
            var result = await _leaveRequestTypeService.GetLeaveRequestTypeById(id);

            if (result == null)
            {
                Log.Warning("LeaveRequestController - Leave request not found for ID: {Id}", id);
                return Ok(new ApiResponse<LeaveRequest>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.LeaveRequestNotFound,
                    Data = null
                });
            }

            Log.Information("LeaveRequestController - Leave request found for ID: {Id}", id);
            return Ok(new ApiResponse<LeaveRequest>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.LeaveRequestFound,
                Data = result
            });
        }


        [HttpPost("AddLeaveRequest")]
        public async Task<IActionResult> Create([FromBody] LeaveRequest leaveRequest)
        {
            Log.Information("LeaveRequestController - AddLeaveRequest called.");

            if (leaveRequest == null)
            {
                Log.Warning("LeaveRequestController - Invalid leave request object.");
                return BadRequest("Invalid data.");
            }

            var result = await _leaveRequestTypeService.AddLeaveRequestType(leaveRequest);
            if (result)
            {
                Log.Information("LeaveRequestController - Leave request added successfully.");
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.LeaveRequestTypeAdded,
                    Data = true
                });
            }

            Log.Warning("LeaveRequestController - Failed to add leave request.");
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.LeaveRequestTypeNotAdded,
                Data = false
            });
        }


        [HttpPut("UpdateLeaveRequest/{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] LeaveRequest leaveRequest)
        {
            Log.Information("LeaveRequestController - UpdateLeaveRequest called for ID: {Id}", leaveRequest.Id);

            if (leaveRequest.Id == null)
            {
                Log.Warning("LeaveRequestController - ID Not Found: URL ID {Id}, Body ID {BodyId}", leaveRequest.Id);
                return Ok("ID Does Not Exist.");
            }

            var result = await _leaveRequestTypeService.UpdateLeaveRequestType(leaveRequest);
            if (result)
            {
                Log.Information("LeaveRequestController - Leave request updated successfully for ID: {Id}", leaveRequest.Id);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.LeaveRequestUpdated,
                    Data = true
                });
            }

            Log.Warning("LeaveRequestController - Failed to update leave request for ID: {Id}", leaveRequest.Id);
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.LeaveRequestNotUpdated,
                Data = false
            });
        }


        [HttpDelete("DeleteLeaveRequest/{id}")]
        [Authorize(Roles = "Admin,System Admin,HR,HR Operations")]
        public async Task<IActionResult> Delete(long id)
        {
            Log.Information("LeaveRequestController - DeleteLeaveRequest called for ID: {Id}", id);

            var result = await _leaveRequestTypeService.DeleteLeaveRequestType(id);
            if (result)
            {
                Log.Information("LeaveRequestController - Leave request deleted for ID: {Id}", id);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.LeaveRequestDeleted,
                    Data = true
                });
            }

            Log.Warning("LeaveRequestController - Failed to delete leave request for ID: {Id}", id);
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.LeaveRequestNotDeleted,
                Data = false
            });
        }


        [HttpPost("ApproveLeaveRequest")]
        [Authorize(Roles = "Admin,System Admin,HR,HR Operations,Manager,Management")]
        public async Task<IActionResult> ApproveLeaveRequest([FromBody] ApproveLeaveRequest leaveRequest)
        {
            Log.Information("ApproveLeaveRequest called for ID: {LeaveRequestId}, Status: {Status}", 
                leaveRequest?.Id, leaveRequest?.Status);

            if (leaveRequest == null)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Invalid leave approval payload.",
                    Data = false
                });
            }

            try
            {
                var result = await _leaveRequestTypeService.ApproveLeaveRequestAsync(leaveRequest);
                if (!result)
                {
                    Log.Warning("Approval/Rejection failed for ID: {Id}", leaveRequest.Id);
                    return BadRequest(new ApiResponse<bool>
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Message = "Action failed. Leave request not found.",
                        Data = false
                    });
                }

                var statusLabel = leaveRequest.Status?.ToLower() ?? "processed";
                Log.Information("LeaveRequest ID {Id} {Status} successfully", leaveRequest.Id, statusLabel);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = $"Leave {statusLabel} successfully.",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing leave ID: {LeaveRequestId}", leaveRequest.Id);
                return StatusCode(500, new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = "Internal server error while processing leave decision.",
                    Data = null
                });
            }
        }

        [HttpGet("GetEmployeeLeaveBalance/{employeeId?}")]
        public async Task<IActionResult> GetEmployeeLeaveBalance(long? employeeId = null)
        {
            try
            {
                var balances = await _leaveRequestTypeService.GetEmployeeLeaveBalances(employeeId);
                return Ok(new ApiResponse<IEnumerable<EmployeeLeaveBalanceDto>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Leave balances fetched successfully.",
                    Data = balances
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while fetching leave balances for Employee ID: {EmployeeId}", employeeId);
                return StatusCode(500, new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = "Internal server error while fetching leave balances.",
                    Data = null
                });
            }
        }
    }
}



