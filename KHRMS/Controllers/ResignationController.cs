using System.Net;
using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace KHRMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResignationController(IResignationService resignationService) : Controller
    {
        private readonly IResignationService _resign = resignationService;
        [HttpGet]
        [Route("GetResignation")]
        public async Task<IActionResult> GetResignation()
        {
            Log.Information("GetResignation API called.");

            var resignations = await _resign.GetAllResignations();
            if (resignations == null || !resignations.Any())
            {
                Log.Information("No Resignation records found.");
                return Ok(new ApiResponse<List<ResignationRequestModel>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.ResignationRequestNotFound,
                    Data = null
                });
            }

            Log.Information("Resignation records found successfully.");
            return Ok(new ApiResponse<List<ResignationRequestModel>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.ResignationRequestFound,
                Data = resignations.ToList()
            });
        }

        [HttpPost]
        [Route("AddResignation")]
        public async Task<IActionResult> AddResignation(ResignationRequestModel resignation)
        {
            Log.Information("AddResignation API called.");

            var isAdded = await _resign.AddResignations(resignation);
            if (!isAdded)
            {
                Log.Warning("Failed to add AddResignation.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.ResignationRequestNotFound,
                    Data = false
                });
            }

            Log.Information("AddResignation added successfully.");
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.ResignationRequestFound,
                Data = true
            });
        }

        [HttpPut]
        [Route("UpdateResignation")]
        public async Task<IActionResult> UpdateResignation(Resignation resignation)
        {
            Log.Information("UpdateResignation API called.");

            var isUpdated = await _resign.UpdateResignation(resignation);
            if (!isUpdated)
            {
                Log.Warning("Failed to update Resignation with ID {ResignationId}.", resignation.Id);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.ResignationRequestNotFound,
                    Data = false
                });
            }

            Log.Information("Resignation with ID {ResignationId} updated successfully.", resignation.Id);
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.ResignationRequestUpdated,
                Data = true
            });
        }


        [HttpDelete]
        [Route("DeleteResignation")]
        public async Task<IActionResult> DeleteResignation(long ResignationId)
        {
            Log.Information("DeleteResignation API called for ID {ResignationId}.", ResignationId);

            var isDeleted = await _resign.DeleteResignation(ResignationId);
            if (!isDeleted)
            {
                Log.Warning("Failed to delete Resignation with ID {ResignationId}.", ResignationId);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.ResignationRequestNotFound,
                    Data = false
                });
            }

            Log.Information("Resignation with ID {ResignationId} deleted successfully.", ResignationId);
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.ResignationRequestDeleted,
                Data = true
            });
        }

        [HttpPut]
        [Route("ApproveOrRejectResignation")]
        public async Task<IActionResult> ApproveOrRejectResignation(Resignation resignation)
        {
            Log.Information("ApproveOrRejectResignation API called.");

            var isUpdated = await _resign.ApproveOrRejectResignation(resignation);
            if (!isUpdated)
            {
                Log.Warning("Failed to update Resignation with ID {ResignationId}.", resignation.Id);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.ResignationRequestNotFound,
                    Data = false
                });
            }

            Log.Information("Resignation with ID {ResignationId} updated successfully.", resignation.Id);
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.ResignationRequestUpdated,
                Data = true
            });
        }
    }
}
