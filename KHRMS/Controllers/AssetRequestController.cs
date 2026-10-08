using System.Net;
using KHRMS.Authorization;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace KHRMS.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AssetRequestController(IAssetRequestService assetRequestService, IUserContextService userContextService) : ControllerBase
    {
        private readonly IAssetRequestService _assetRequestService = assetRequestService;
        private readonly IUserContextService _userContextService = userContextService;

        [HttpPost("CreateRequest")]
        [RequirePermission("ASSET_REQUEST_SELF")]
        public async Task<IActionResult> CreateRequest([FromBody] CreateAssetRequestModel model)
        {
            Log.Information("CreateRequest API called for AssetId: {AssetId}, EmployeeId: {EmployeeId}", model?.AssetId, model?.EmployeeId);

            if (model == null)
            {
                return BadRequest(new ApiResponse<AssetRequestResponseModel>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Invalid request payload.",
                    Data = null
                });
            }

            var result = await _assetRequestService.CreateAssetRequest(model);
            if (result != null)
            {
                return Ok(new ApiResponse<AssetRequestResponseModel>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Asset request submitted successfully.",
                    Data = result
                });
            }

            return BadRequest(new ApiResponse<AssetRequestResponseModel>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = "Failed to submit asset request. Ensure Asset and Employee exist.",
                Data = null
            });
        }

        [HttpPut("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateAssetRequestStatusModel model)
        {
            Log.Information("UpdateStatus API called for RequestId: {RequestId}, NewStatus: {NewStatus}", model?.RequestId, model?.NewStatus);

            if (model == null || model.RequestId <= 0)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Invalid update payload.",
                    Data = false
                });
            }

            var hasApprovePerm = await _userContextService.HasPermissionAsync("ASSET_APPROVE");
            if (!hasApprovePerm)
            {
                // Employee self-service check: User must have ASSET_REQUEST_SELF permission
                var hasSelfPerm = await _userContextService.HasPermissionAsync("ASSET_REQUEST_SELF");
                if (!hasSelfPerm)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse<bool>
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = "Access Denied: Missing required permission 'ASSET_APPROVE' or 'ASSET_REQUEST_SELF'.",
                        Data = false
                    });
                }

                // Verify this ticket belongs to the current employee
                var currentEmpId = _userContextService.GetCurrentEmployeeId();
                var req = await _assetRequestService.GetAssetRequestById(model.RequestId);
                if (req == null || (currentEmpId > 0 && req.EmployeeId != currentEmpId))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse<bool>
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = "Access Denied: You can only update your own asset service tickets.",
                        Data = false
                    });
                }

                // Verify the requested status transition is a valid employee self-service action:
                // 1. Acknowledging / closing rejected ticket ("Closed")
                // 2. Handover / Courier dispatch ("Handed Over at IT Desk", "In Transit", "In Transit (Picked Up)")
                // 3. Confirming receipt / delivery ("Completed", "Received")
                // 4. Confirming desk handover received ("In Repair", "Received by Admin")
                var normStatus = model.NewStatus?.Trim().ToLowerInvariant() ?? "";
                bool isAllowedEmployeeStatus = normStatus.Contains("closed")
                    || normStatus.Contains("transit")
                    || normStatus.Contains("desk")
                    || normStatus.Contains("handover")
                    || normStatus.Contains("received")
                    || normStatus.Contains("repair")
                    || normStatus.Contains("completed");

                if (!isAllowedEmployeeStatus)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse<bool>
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = "Access Denied: Administrative approvals require 'ASSET_APPROVE' permission.",
                        Data = false
                    });
                }
            }

            var isUpdated = await _assetRequestService.UpdateAssetRequestStatus(model);
            if (isUpdated)
            {
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = $"Asset request status updated to '{model.NewStatus}' successfully.",
                    Data = true
                });
            }

            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = "Failed to update asset request status.",
                Data = false
            });
        }

        [HttpGet("GetRequests")]
        public async Task<IActionResult> GetRequests([FromQuery] long? employeeId = null)
        {
            Log.Information("GetRequests API called. Filter employeeId: {EmployeeId}", employeeId);

            var requests = await _assetRequestService.GetAssetRequests(employeeId);
            return Ok(new ApiResponse<List<AssetRequestResponseModel>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = requests.Count > 0 ? "Asset requests found successfully." : "No asset requests found.",
                Data = requests
            });
        }

        [HttpGet("GetRequestById/{id}")]
        public async Task<IActionResult> GetRequestById(long id)
        {
            Log.Information("GetRequestById API called for ID: {Id}", id);

            if (id <= 0)
            {
                return BadRequest(new ApiResponse<AssetRequestResponseModel>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Invalid ID.",
                    Data = null
                });
            }

            var request = await _assetRequestService.GetAssetRequestById(id);
            if (request != null)
            {
                return Ok(new ApiResponse<AssetRequestResponseModel>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Asset request found successfully.",
                    Data = request
                });
            }

            return NotFound(new ApiResponse<AssetRequestResponseModel>
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Message = "Asset request not found.",
                Data = null
            });
        }

        [HttpPost("UploadImages")]
        public async Task<IActionResult> UploadImages([FromForm] List<IFormFile> files)
        {
            Log.Information("UploadImages API called with {Count} file(s)", files?.Count ?? 0);

            if (files == null || files.Count == 0)
            {
                return BadRequest(new ApiResponse<List<string>>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "No files uploaded.",
                    Data = new List<string>()
                });
            }

            var uploadedUrls = await _assetRequestService.UploadAssetImages(files);
            return Ok(new ApiResponse<List<string>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = $"{uploadedUrls.Count} file(s) uploaded successfully.",
                Data = uploadedUrls
            });
        }
    }
}
