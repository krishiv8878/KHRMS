using KHRMS.Authorization;
using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
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
    public class AssetsMasterController(IAssetsMasterService assetsMasterService, IUserContextService userContextService) : ControllerBase
    {
        private readonly IAssetsMasterService _assetsMasterService = assetsMasterService;
        private readonly IUserContextService _userContext = userContextService;

        [HttpGet]
        [Route("GetAssetsMaster")]
        public async Task<IActionResult> GetAssetsMaster()
        {
            Log.Information("GetAssetsMaster API called.");

            var assetsMaster = await _assetsMasterService.GetAllAssetsMaster();
            if (assetsMaster == null || !assetsMaster.Any())
            {
                Log.Information("No AssetsMaster records found.");
                return Ok(new ApiResponse<List<AssetsMaster>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.AssetsMasterNotFound,
                    Data = null
                });
            }

            var canViewAll = await _userContext.HasPermissionAsync("ASSET_VIEW_ALL");
            if (!canViewAll)
            {
                var currentEmpId = _userContext.GetCurrentEmployeeId();
                if (currentEmpId > 0)
                {
                    assetsMaster = assetsMaster.Where(a => a.EmployeeId == currentEmpId).ToList();
                }
                else
                {
                    assetsMaster = new List<AssetsMaster>();
                }
            }

            Log.Information("AssetsMaster records found successfully.");
            return Ok(new ApiResponse<List<AssetsMaster>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AssetsMasterFound,
                Data = assetsMaster.ToList()
            });
        }

        [HttpPost]
        [Route("AddAssetsMaster")]
        [RequirePermission("ASSET_MANAGE")]
        public async Task<IActionResult> AddAssetsMaster(AssetsMaster assetsMaster)
        {
            Log.Information("AddAssetsMaster API called.");

            var isAdded = await _assetsMasterService.AddAssetsMaster(assetsMaster);
            if (!isAdded)
            {
                Log.Warning("Failed to add AssetsMaster.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.AssetsMasterNotAdded,
                    Data = false
                });
            }

            Log.Information("AssetsMaster added successfully.");
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AssetsMasterAdded,
                Data = true
            });
        }

        [HttpPut]
        [Route("UpdateAssetsMaster")]
        [RequirePermission("ASSET_MANAGE")]
        public async Task<IActionResult> UpdateAssetsMaster(AssetsMaster assetsMaster)
        {
            Log.Information("UpdateAssetsMaster API called.");

            var isUpdated = await _assetsMasterService.UpdateAssetsMaster(assetsMaster);
            if (!isUpdated)
            {
                Log.Warning("Failed to update AssetsMaster with ID {AssetsMasterId}.", assetsMaster.Id);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.AssetsMasterNotFound,
                    Data = false
                });
            }

            Log.Information("AssetsMaster with ID {AssetsMasterId} updated successfully.", assetsMaster.Id);
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AssetsMasterUpdated,
                Data = true
            });
        }


        [HttpDelete]
        [Route("DeleteAssetsMaster")]
        [RequirePermission("ASSET_MANAGE")]
        public async Task<IActionResult> DeleteAssetsMaster(long AssetsMasterId)
        {
            Log.Information("DeleteAssetsMaster API called for ID {AssetsMasterId}.", AssetsMasterId);

            var isDeleted = await _assetsMasterService.DeleteAssetsMaster(AssetsMasterId);
            if (!isDeleted)
            {
                Log.Warning("Failed to delete AssetsMaster with ID {AssetsMasterId}.", AssetsMasterId);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.AssetsMasterNotFound,
                    Data = false
                });
            }

            Log.Information("AssetsMaster with ID {AssetsMasterId} deleted successfully.", AssetsMasterId);
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AssetsMasterDeleted,
                Data = true
            });
        }
    }
}
