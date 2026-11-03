using KHRMS.Authorization;
using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace KHRMS.Controllers
{
    public class AssignEmployeeRolesRequest
    {
        public long EmployeeId { get; set; }
        public List<long> RoleIds { get; set; } = new();
        public string Reason { get; set; } = string.Empty;
    }

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeRoleMappingController(IEmployeeRoleMappingService employeeRoleMappingService) : ControllerBase
    {
        public readonly IEmployeeRoleMappingService _employeeRoleMappingService = employeeRoleMappingService;

        /// <summary>
        /// Get List Of EmployeeRoleMapping
        /// </summary>
        [HttpGet("GetEmployeeRoles")]
        public async Task<IActionResult> GetEmployeeRoles()
        {
            Log.Information("EmployeeRoleMappingController - GetEmployeeRoles called.");
            var employeeroleMapping = await _employeeRoleMappingService.GetAllEmployeeRoleMapping();

            if (employeeroleMapping == null || !employeeroleMapping.Any())
            {
                Log.Warning("EmployeeRoleMappingController - No employee role mappings found.");
                return Ok(new ApiResponse<List<EmployeeRoleMapping>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.NoEmployeeRoleMappingFound,
                    Data = null
                });
            }

            Log.Information("EmployeeRoleMappingController - {Count} employee role mappings found.", employeeroleMapping.Count());
            return Ok(new ApiResponse<List<EmployeeRoleMapping>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmployeeRoleMapping,
                Data = employeeroleMapping.ToList()
            });
        }

        /// <summary>
        /// Assign multiple roles to an employee with mandatory reason and safeguard validations
        /// </summary>
        [HttpPost("AssignEmployeeRoles")]
        [RequirePermission("ROLE_ASSIGN")]
        public async Task<IActionResult> AssignEmployeeRoles([FromBody] AssignEmployeeRolesRequest request)
        {
            if (request == null || request.EmployeeId <= 0)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Valid employee ID is required.",
                    Data = false
                });
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "A mandatory business reason is required for role modification.",
                    Data = false
                });
            }

            try
            {
                var result = await _employeeRoleMappingService.AssignRolesToEmployeeAsync(request.EmployeeId, request.RoleIds, request.Reason);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Employee roles updated successfully.",
                    Data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                Log.Warning(ex, "Safeguard blocked AssignEmployeeRoles for employee {EmployeeId}", request.EmployeeId);
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = false
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error in AssignEmployeeRoles");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Failed to assign roles.",
                    Data = false
                });
            }
        }

        /// <summary>
        /// Add New EmployeeRoleMapping
        /// </summary>
        [HttpPost("AssignEmployeeRole")]
        [RequirePermission("ROLE_ASSIGN")]
        public async Task<IActionResult> AssignEmployeeRole([FromBody] EmployeeRoleMapping employeeRoleMapping)
        {
            Log.Information("EmployeeRoleMappingController - AssignEmployeeRole called.");
            try
            {
                var result = await _employeeRoleMappingService.CreateEmployeeRoleMapping(employeeRoleMapping);

                if (result)
                {
                    Log.Information("EmployeeRoleMappingController - Employee role mapping assigned successfully.");
                    return Ok(new ApiResponse<bool>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = ApiMessageConstant.EmployeeRoleMappingAdded,
                        Data = true
                    });
                }

                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.EmployeeRoleMappingNotAdded,
                    Data = false
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = false
                });
            }
        }

        /// <summary>
        /// Update EmployeeRoleMapping
        /// </summary>
        [HttpPut("UpdateEmployeeRole")]
        [RequirePermission("ROLE_ASSIGN")]
        public async Task<IActionResult> UpdateEmployeeRole([FromBody] EmployeeRoleMapping employeeRoleMapping)
        {
            Log.Information("EmployeeRoleMappingController - UpdateEmployeeRole called.");
            try
            {
                var result = await _employeeRoleMappingService.UpdateEmployeeRoleMapping(employeeRoleMapping);

                if (result)
                {
                    Log.Information("EmployeeRoleMappingController - Employee role mapping updated successfully.");
                    return Ok(new ApiResponse<bool>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = ApiMessageConstant.EmployeeRoleMappingUpdated,
                        Data = true
                    });
                }

                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.EmployeeRoleMappingNotUpdated,
                    Data = false
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = false
                });
            }
        }

        /// <summary>
        /// Delete EmployeeRoleMapping
        /// </summary>
        [HttpDelete("DeleteEmployeeRole")]
        [RequirePermission("ROLE_ASSIGN")]
        public async Task<IActionResult> DeleteEmployeeRole(long employeeRoleMappingId)
        {
            Log.Information("EmployeeRoleMappingController - DeleteEmployeeRole called with ID: {Id}", employeeRoleMappingId);
            try
            {
                var result = await _employeeRoleMappingService.DeleteEmployeeRoleMapping(employeeRoleMappingId);

                if (result)
                {
                    Log.Information("EmployeeRoleMappingController - Employee role mapping deleted successfully for ID: {Id}", employeeRoleMappingId);
                    return Ok(new ApiResponse<bool>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = ApiMessageConstant.EmployeeRoleMappingDeleted,
                        Data = true
                    });
                }

                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.EmployeeRoleMappingNotDeleted,
                    Data = false
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = false
                });
            }
        }
    }
}
