using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;

namespace KHRMS.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;
        private readonly IUserContextService _userContext;

        public PermissionController(IPermissionService permissionService, IUserContextService userContext)
        {
            _permissionService = permissionService;
            _userContext = userContext;
        }

        public class SaveRolePermissionsRequest
        {
            public long RoleId { get; set; }
            public List<long> PermissionIds { get; set; } = new();
        }

        /// <summary>
        /// Get all registered system permissions grouped by module
        /// </summary>
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            Log.Information("PermissionController - GetAll called");
            var permissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(new ApiResponse<IEnumerable<PermissionMaster>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Permissions retrieved successfully.",
                Data = permissions
            });
        }

        /// <summary>
        /// Get permission IDs assigned to a specific role
        /// </summary>
        [HttpGet("GetRolePermissions/{roleId}")]
        public async Task<IActionResult> GetRolePermissions(long roleId)
        {
            Log.Information("PermissionController - GetRolePermissions called for RoleId {RoleId}", roleId);
            var permissionIds = await _permissionService.GetRolePermissionIdsAsync(roleId);
            return Ok(new ApiResponse<List<long>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Role permissions retrieved successfully.",
                Data = permissionIds
            });
        }

        /// <summary>
        /// Update permissions for a specific role
        /// </summary>
        [HttpPost("SaveRolePermissions")]
        [Authorize(Roles = "Admin,System Admin,HR,HR Operations")]
        public async Task<IActionResult> SaveRolePermissions([FromBody] SaveRolePermissionsRequest request)
        {
            if (request == null || request.RoleId <= 0)
            {
                return BadRequest(new ApiResponse<string>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Valid RoleId is required.",
                    Data = null
                });
            }

            Log.Information("PermissionController - SaveRolePermissions called for RoleId {RoleId} with {Count} permissions", request.RoleId, request.PermissionIds.Count);
            var success = await _permissionService.SaveRolePermissionsAsync(request.RoleId, request.PermissionIds);

            if (!success)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<string>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Failed to update role permissions.",
                    Data = null
                });
            }

            return Ok(new ApiResponse<string>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Role permissions updated successfully.",
                Data = null
            });
        }

        /// <summary>
        /// Get effective permission codes for the currently authenticated user
        /// </summary>
        [HttpGet("MyPermissions")]
        public async Task<IActionResult> GetMyPermissions()
        {
            var empId = _userContext.GetCurrentEmployeeId();
            var perms = await _permissionService.GetUserEffectivePermissionsAsync(empId);
            return Ok(new ApiResponse<List<string>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "My permissions retrieved successfully.",
                Data = perms
            });
        }

        /// <summary>
        /// Get effective permissions for any employee (Admin/HR only)
        /// </summary>
        [HttpGet("GetUserPermissions/{employeeId}")]
        [Authorize(Roles = "Admin,System Admin,HR,HR Operations,Manager,Management")]
        public async Task<IActionResult> GetUserPermissions(long employeeId)
        {
            var perms = await _permissionService.GetUserEffectivePermissionsAsync(employeeId);
            return Ok(new ApiResponse<List<string>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "User permissions retrieved successfully.",
                Data = perms
            });
        }
    }
}
