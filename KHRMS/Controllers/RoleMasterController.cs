using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KHRMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleMasterController(IRoleMasterService roleMasterService) : ControllerBase
    {
        public readonly IRoleMasterService _roleMasterService = roleMasterService;
        /// <summary>
        /// Get List Of RoleMaster
        /// </summary>
        /// <returns></returns>
        //[HttpGet]
        //[Route("GetRoles")]
        //public async Task<IActionResult> GetRoles()
        //{
        //    var rolemaster = await _roleMasterService.GetAllRoleMaster();
        //    if (rolemaster == null)
        //    {
        //        return NotFound();
        //    }
        //    var response = new ApiResponse<List<RoleMaster>>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = rolemaster.Any() ? ApiMessageConstant.RoleMasterFound : ApiMessageConstant.NoRoleMasterFound,
        //        Data = rolemaster.ToList()
        //    };

        //    return Ok(response);
        //}
        [HttpGet("GetRoles")]
        public async Task<IActionResult> GetRoles()
        {
            Log.Information("RoleMasterController - GetRoles called.");

            var roles = await _roleMasterService.GetAllRoleMaster();

            if (roles == null || !roles.Any())
            {
                Log.Warning("RoleMasterController - No roles found.");
                return NotFound(new ApiResponse<List<RoleMaster>>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
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
        /// <param name="roleMaster"></param>
        /// <returns></returns>
        //[HttpPost]
        //[Route("AddRole")]
        //public async Task<IActionResult> AddRole(RoleMaster roleMaster)
        //{
        //    var isRoleMasterAdded = await _roleMasterService.AddRoleMaster(roleMaster);
        //    if (isRoleMasterAdded)
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.RoleMasterAdded,
        //            Data = isRoleMasterAdded
        //        };
        //        return Ok(response);
        //    }
        //    else
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.RoleMasterNotAdded,
        //            Data = isRoleMasterAdded
        //        };
        //        return BadRequest(response);
        //    }
        //}
        [HttpPost("AddRole")]
        public async Task<IActionResult> AddRole(RoleMaster roleMaster)
        {
            Log.Information("RoleMasterController - AddRole called.");

            if (roleMaster == null)
            {
                Log.Warning("RoleMasterController - Invalid roleMaster object.");
                return BadRequest("Invalid role data.");
            }

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

            Log.Warning("RoleMasterController - Failed to add role.");
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.RoleMasterNotAdded,
                Data = false
            });
        }

        /// <summary>
        /// Update RoleMaster
        /// </summary>
        /// <param name="roleMaster"></param>
        /// <returns></returns>
        //[HttpPut]
        //[Route("UpdateRole")]
        //public async Task<IActionResult> UpdateRole(RoleMaster roleMaster)
        //{
        //    var isRoleMasterUpdated = await _roleMasterService.UpdateRoleMaster(roleMaster);
        //    if (isRoleMasterUpdated)
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.RoleMasterUpdated,
        //            Data = isRoleMasterUpdated
        //        };
        //        return Ok(response);
        //    }
        //    else
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.RoleMasterNotUpdated,
        //            Data = isRoleMasterUpdated
        //        };
        //        return BadRequest(response);
        //    }
        //}
        [HttpPut("UpdateRole/{id}")]
        public async Task<IActionResult> UpdateRole(long id, RoleMaster roleMaster)
        {
            Log.Information("RoleMasterController - UpdateRole called for ID: {Id}", id);

            if (id != roleMaster.Id)
            {
                Log.Warning("RoleMasterController - ID mismatch: URL ID {Id}, Body ID {BodyId}", id, roleMaster.Id);
                return BadRequest("ID mismatch.");
            }

            var result = await _roleMasterService.UpdateRoleMaster(roleMaster);

            if (result)
            {
                Log.Information("RoleMasterController - Role updated successfully for ID: {Id}", id);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.RoleMasterUpdated,
                    Data = true
                });
            }

            Log.Warning("RoleMasterController - Failed to update role for ID: {Id}", id);
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.RoleMasterNotUpdated,
                Data = false
            });
        }

        /// <summary>
        /// Delete RoleMaster
        /// </summary>
        /// <param name="RoleId"></param>
        /// <returns></returns>
        //[HttpDelete]
        //[Route("DeleteRole")]
        //public async Task<IActionResult> DeleteRole(long RoleId)
        //{
        //    var isRoleMasterDeleted = await _roleMasterService.DeleteRoleMaster(RoleId);
        //    if (isRoleMasterDeleted)
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.RoleMasterDeleted,
        //            Data = isRoleMasterDeleted
        //        };
        //        return Ok(response);
        //    }
        //    else
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.RoleMasterNotDeleted,
        //            Data = isRoleMasterDeleted
        //        };
        //        return BadRequest(response);
        //    }
        //}
        [HttpDelete("DeleteRole/{id}")]
        public async Task<IActionResult> DeleteRole(long id)
        {
            Log.Information("RoleMasterController - DeleteRole called for ID: {Id}", id);

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

            Log.Warning("RoleMasterController - Failed to delete role for ID: {Id}", id);
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.RoleMasterNotDeleted,
                Data = false
            });
        }
    }
}
