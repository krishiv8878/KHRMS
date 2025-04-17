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
    public class DesignationController(IDesignationService designationService) : ControllerBase
    {
        public readonly IDesignationService _designationService = designationService;


        [HttpGet("GetDesignations")]
        public async Task<IActionResult> GetDesignations()
        {
            Log.Information("GetDesignations API called.");
            var designations = await _designationService.GetAllDesignations();
            if (designations == null || !designations.Any())
            {
                Log.Information("No designations found.");
                return NotFound(new ApiResponse<List<Designation>>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ApiMessageConstant.DesignationNotFound,
                    Data = null
                });
            }

            Log.Information("Designations retrieved successfully.");
            return Ok(new ApiResponse<List<Designation>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.DesgnationFound,
                Data = designations.ToList()
            });
        }

        [HttpPost("AddDesignation")]
        public async Task<IActionResult> AddDesignation(Designation designation)
        {
            Log.Information("AddDesignation API called.");
            var isDesignationAdded = await _designationService.CreateDesignation(designation);
            if (!isDesignationAdded)
            {
                Log.Warning("Failed to add designation.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.DesignationNotAdded,
                    Data = false
                });
            }

            Log.Information("Designation added successfully.");
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.DesignationAdded,
                Data = true
            });
        }


        [HttpPut("UpdateDesignation")]
        public async Task<IActionResult> UpdateDesignation(Designation designation)
        {
            Log.Information("UpdateDesignation API called.");
            var isDesignationEdited = await _designationService.UpdateDesignation(designation);
            if (!isDesignationEdited)
            {
                Log.Warning("Failed to update designation.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.DesignationNotUpdated,
                    Data = false
                });
            }

            Log.Information("Designation updated successfully.");
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.DesignationUpdated,
                Data = true
            });
        }


        [HttpDelete("DeleteDesignation")]
        public async Task<IActionResult> DeleteDesignation(long designationId)
        {
            Log.Information("DeleteDesignation API called for ID {DesignationId}.", designationId);
            var isDesignationDeleted = await _designationService.DeleteDesignation(designationId);
            if (!isDesignationDeleted)
            {
                Log.Warning("Failed to delete designation with ID {DesignationId}.", designationId);
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.DesignationNotDeleted,
                    Data = false
                });
            }

            Log.Information("Designation with ID {DesignationId} deleted successfully.", designationId);
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.DesignationDeleted,
                Data = true
            });
        }
    }
}
