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


        //[HttpGet]
        //[Route("GetDesignations")]
        //public async Task<IActionResult> GetDesignations()
        //{
        //    var designations = await _designationService.GetAllDesignations();
        //    if (designations == null)
        //    {
        //        return NotFound();
        //    }
        //    // Use the wrapper class to create a consistent response
        //    var response = new ApiResponse<List<Designation>>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = designations.Any() ? ApiMessageConstant.DesgnationFound : ApiMessageConstant.DesignationNotFound,
        //        Data = designations.ToList()
        //    };
        //    return Ok(response);
        //}
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


        //[HttpPost]
        //[Route("AddDesignation")]
        //public async Task<IActionResult> AddDesignation(Designation designation)
        //{
        //    var isDesignationAdded = await _designationService.CreateDesignation(designation);
        //    if (isDesignationAdded)
        //    {
        //        // Use the wrapper class to create a consistent response
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.DesignationAdded,
        //            Data = isDesignationAdded
        //        };
        //        return Ok(response);
        //    }
        //    else
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.DesignationNotAdded,
        //            Data = isDesignationAdded
        //        };
        //        return BadRequest(response);
        //    }
        //}
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


        //[HttpPut]
        //[Route("UpdateDesignation")]
        //public async Task<IActionResult> UpdateDesignation(Designation designation)
        //{
        //    var isDesignationEdited = await _designationService.UpdateDesignation(designation);
        //    if (isDesignationEdited)
        //    {
        //        // Use the wrapper class to create a consistent response
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.DesignationUpdated,
        //            Data = isDesignationEdited
        //        };
        //        return Ok(response);
        //    }
        //    else
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.DesignationNotUpdated,
        //            Data = isDesignationEdited
        //        };
        //        return BadRequest(response);
        //    }
        //}

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



        //[HttpDelete]
        //[Route("DeleteDesignation")]
        //public async Task<IActionResult> DeleteDesignation(long designationId)
        //{
        //    var isDesignationDeleted = await _designationService.DeleteDesignation(designationId);
        //    if (isDesignationDeleted)
        //    {
        //        // Use the wrapper class to create a consistent response
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.DesignationDeleted,
        //            Data = isDesignationDeleted
        //        };
        //        return Ok(response);
        //    }
        //    else
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.DesignationNotDeleted,
        //            Data = isDesignationDeleted
        //        };
        //        return BadRequest(response);
        //    }
        //}

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
