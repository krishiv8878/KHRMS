using Azure;
using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;

namespace KHRMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeRoleMappingController(IEmployeeRoleMappingService employeeRoleMappingService) : ControllerBase
    {
        public readonly IEmployeeRoleMappingService _employeeRoleMappingService = employeeRoleMappingService;
        /// <summary>
        /// Get List Of EmployeeRoleMapping
        /// </summary>
        /// <returns></returns>
        //[HttpGet]
        //[Route("GetEmployeeRoles")]
        //public async Task<IActionResult> GetEmployeeRoles()
        //{
        //    var employeeroleMapping = await _employeeRoleMappingService.GetAllEmployeeRoleMapping();
        //    if (employeeroleMapping == null)
        //    {
        //        return NotFound();
        //    }
        //    var response = new ApiResponse<List<EmployeeRoleMapping>>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = employeeroleMapping.Any() ? ApiMessageConstant.EmployeeRoleMapping: ApiMessageConstant.NoEmployeeRoleMappingFound,
        //        Data = employeeroleMapping.ToList()
        //    };
        //    return Ok(response);
        //}
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
        /// Add New EmployeeRoleMapping
        /// </summary>
        /// <param name="employeeRoleMapping"></param>
        /// <returns></returns>
        //[HttpPost]
        //[Route("AssignEmployeeRole")]
        //public async Task<IActionResult> AssignEmployeeRole(EmployeeRoleMapping employeeRoleMapping)
        //{
        //    var isEmployeRoleMappingAdded = await _employeeRoleMappingService.CreateEmployeeRoleMapping(employeeRoleMapping);
        //    if (isEmployeRoleMappingAdded)
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.EmployeeRoleMappingAdded,
        //            Data = isEmployeRoleMappingAdded
        //        };
        //        return Ok(response);
        //    }
        //    else
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.EmployeeRoleMappingNotAdded,
        //            Data = isEmployeRoleMappingAdded
        //        };
        //        return BadRequest(response);
        //    }
        //}

        [HttpPost("AssignEmployeeRole")]
        public async Task<IActionResult> AssignEmployeeRole([FromBody] EmployeeRoleMapping employeeRoleMapping)
        {
            Log.Information("EmployeeRoleMappingController - AssignEmployeeRole called.");
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

            Log.Warning("EmployeeRoleMappingController - Failed to assign employee role mapping.");
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.EmployeeRoleMappingNotAdded,
                Data = false
            });
        }
        /// <summary>
        /// Update EmployeeRoleMapping
        /// </summary>
        /// <param name="employeeRoleMapping"></param>
        /// <returns></returns>
        //[HttpPut]
        //[Route("UpdateEmployeeRole")]
        //public async Task<IActionResult> UpdateEmployeeRole(EmployeeRoleMapping employeeRoleMapping)
        //{
        //    var isEmployeRoleMappingUpdated = await _employeeRoleMappingService.UpdateEmployeeRoleMapping(employeeRoleMapping);
        //    if (isEmployeRoleMappingUpdated)
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.EmployeeRoleMappingUpdated,
        //            Data = isEmployeRoleMappingUpdated
        //        };
        //        return Ok(response);
        //    }
        //    else
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.EmployeeRoleMappingNotUpdated,
        //            Data = isEmployeRoleMappingUpdated
        //        };
        //        return BadRequest(response);
        //    }

        //}
        [HttpPut("UpdateEmployeeRole")]
        public async Task<IActionResult> UpdateEmployeeRole([FromBody] EmployeeRoleMapping employeeRoleMapping)
        {
            Log.Information("EmployeeRoleMappingController - UpdateEmployeeRole called.");
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

            Log.Warning("EmployeeRoleMappingController - Failed to update employee role mapping.");
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.EmployeeRoleMappingNotUpdated,
                Data = false
            });
        }
        /// <summary>
        /// Delete EmployeeRoleMapping
        /// </summary>
        /// <param name="employeeRoleMappingId"></param>
        /// <returns></returns>
        //[HttpDelete]
        //[Route("DeleteEmployeeRole")]
        //public async Task<IActionResult> DeleteEmployeeRole(long employeeRoleMappingId)
        //{
        //    var isEmployeRoleMappingDeleted = await _employeeRoleMappingService.DeleteEmployeeRoleMapping(employeeRoleMappingId);
        //    if (isEmployeRoleMappingDeleted)
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.EmployeeRoleMappingDeleted,
        //            Data = isEmployeRoleMappingDeleted
        //        };
        //        return Ok(response);
        //    }
        //    else
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.EmployeeRoleMappingNotDeleted,
        //            Data = isEmployeRoleMappingDeleted
        //        };
        //        return BadRequest(response);
        //    }
        //}

        [HttpDelete("DeleteEmployeeRole")]
        public async Task<IActionResult> DeleteEmployeeRole(long employeeRoleMappingId)
        {
            Log.Information("EmployeeRoleMappingController - DeleteEmployeeRole called with ID: {Id}", employeeRoleMappingId);
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

            Log.Warning("EmployeeRoleMappingController - Failed to delete employee role mapping for ID: {Id}", employeeRoleMappingId);
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.EmployeeRoleMappingNotDeleted,
                Data = false
            });
        }
    }
}
