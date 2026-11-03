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
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RoleMasterController(IRoleMasterService roleMasterService) : ControllerBase
    {
        public readonly IRoleMasterService _roleMasterService = roleMasterService;

        /// <summary>
        /// Get List Of RoleMaster
        /// </summary>
        [HttpGet("GetRoles")]
        [RequirePermission("ROLE_VIEW")]
        public async Task<IActionResult> GetRoles()
        {
            Log.Information("RoleMasterController - GetRoles called.");

            var roles = await _roleMasterService.GetAllRoleMaster();

            if (roles == null || !roles.Any())
            {
                Log.Warning("RoleMasterController - No roles found.");
                return Ok(new ApiResponse<List<RoleMaster>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.NoRoleMasterFound,
                    Data = null
                });
            }

            Log.Information("RoleMasterController - {Count} roles found.", roles.Count());
            return Ok(new ApiResponse<List<RoleMaster>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.RoleMasterFound,
                Data = roles.ToList()
            });
        }

        /// <summary>
        /// Add New RoleMaster
        /// </summary>
        [HttpPost("AddRole")]
        [RequirePermission("ROLE_MANAGE")]
        public async Task<IActionResult> AddRole(RoleMaster roleMaster)
        {
            Log.Information("RoleMasterController - AddRole called.");

            if (roleMaster == null)
            {
                Log.Warning("RoleMasterController - Invalid roleMaster object.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Invalid role data.",
                    Data = false
                });
            }

            try
            {
                var result = await _roleMasterService.AddRoleMaster(roleMaster);

                if (result)
                {
                    Log.Information("RoleMasterController - Role added successfully.");
                    return Ok(new ApiResponse<bool>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = ApiMessageConstant.RoleMasterAdded,
                        Data = true
                    });
                }

                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.RoleMasterNotAdded,
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
        /// Update RoleMaster
        /// </summary>
        [HttpPut("UpdateRole/{id}")]
        [RequirePermission("ROLE_MANAGE")]
        public async Task<IActionResult> UpdateRole(RoleMaster roleMaster)
        {
            Log.Information("RoleMasterController - UpdateRole called for ID: {Id}", roleMaster?.Id);

            if (roleMaster == null || roleMaster.Id <= 0)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Valid Role ID is required.",
                    Data = false
                });
            }

            try
            {
                var result = await _roleMasterService.UpdateRoleMaster(roleMaster);

                if (result)
                {
                    Log.Information("RoleMasterController - Role updated successfully for ID: {Id}", roleMaster.Id);
                    return Ok(new ApiResponse<bool>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = ApiMessageConstant.RoleMasterUpdated,
                        Data = true
                    });
                }

                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.RoleMasterNotUpdated,
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
        /// Delete RoleMaster
        /// </summary>
        [HttpDelete("DeleteRole/{id}")]
        [RequirePermission("ROLE_MANAGE")]
        public async Task<IActionResult> DeleteRole(long id)
        {
            Log.Information("RoleMasterController - DeleteRole called for ID: {Id}", id);

            try
            {
                var result = await _roleMasterService.DeleteRoleMaster(id);

                if (result)
                {
                    Log.Information("RoleMasterController - Role deleted for ID: {Id}", id);
                    return Ok(new ApiResponse<bool>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = ApiMessageConstant.RoleMasterDeleted,
                        Data = true
                    });
                }

                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.RoleMasterNotDeleted,
                    Data = false
                });
            }
            catch (InvalidOperationException ex)
            {
                Log.Warning(ex, "Safeguard blocked DeleteRole for RoleId {RoleId}", id);
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
