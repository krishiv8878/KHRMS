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
    public class AssetsMasterController(IAssetsMasterService assetsMasterService) : ControllerBase
    {
        private readonly IAssetsMasterService _assetsMasterService = assetsMasterService;
        //[HttpGet]
        //[Route("GetAssetsMaster")]
        //public async Task<IActionResult> GetAssetsMaster()
        //{
        //    try
        //    {
        //        Log.Information("GetAssetsMaster API called.");

        //        var assetsMaster = await _assetsMasterService.GetAllAssetsMaster();
        //        if (assetsMaster == null || !assetsMaster.Any())
        //        {
        //            Log.Information("No AssetsMaster records found.");
        //            return NotFound(new ApiResponse<List<AssetsMaster>>
        //            {
        //                StatusCode = (int)HttpStatusCode.NotFound,
        //                Message = ApiMessageConstant.AssetsMasterNotFound,
        //                Data = null
        //            });
        //        }

        //        Log.Information("AssetsMaster records found successfully.");
        //        return Ok(new ApiResponse<List<AssetsMaster>>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.AssetsMasterFound,
        //            Data = assetsMaster.ToList()
        //        });
        //    }

        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, "Error occurred in GetAssetsMaster. ex{0}: {Message}, ex{1}: {StackTrace}, ex{2}: {InnerException}",
        //            ex.Message, ex.StackTrace, ex.InnerException?.Message ?? "N/A");

        //        return StatusCode(500, new ApiResponse<string>
        //        {
        //            StatusCode = 500,
        //            Message = "An error occurred while processing the request.",
        //            Data = $"ex{{0}}: {ex.Message}, ex{{1}}: {ex.StackTrace}, ex{{2}}: {ex.InnerException?.Message ?? "N/A"}"
        //        });
        //    }

        //}
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

            Log.Information("AssetsMaster records found successfully.");
            return Ok(new ApiResponse<List<AssetsMaster>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AssetsMasterFound,
                Data = assetsMaster.ToList()
            });
        }
        //[HttpPost]
        //[Route("AddAssetsMaster")]
        //public async Task<IActionResult> AddAssetsMaster(AssetsMaster assetsMaster)
        //{
        //    try
        //    {
        //        Log.Information("AddAssetsMaster API called.");

        //        var isAdded = await _assetsMasterService.AddAssetsMaster(assetsMaster);
        //        if (!isAdded)
        //        {
        //            Log.Warning("Failed to add AssetsMaster.");
        //            return BadRequest(new ApiResponse<bool>
        //            {
        //                StatusCode = (int)HttpStatusCode.BadRequest,
        //                Message = ApiMessageConstant.AssetsMasterNotAdded,
        //                Data = false
        //            });
        //        }

        //        Log.Information("AssetsMaster added successfully.");
        //        return Ok(new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.AssetsMasterAdded,
        //            Data = true
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, "Error occurred in AddAssetsMaster. ex{0}: {Message}, ex{1}: {StackTrace}, ex{2}: {InnerException}",
        //            ex.Message, ex.StackTrace, ex.InnerException?.Message ?? "N/A");

        //        return StatusCode(500, new ApiResponse<string>
        //        {
        //            StatusCode = 500,
        //            Message = "An error occurred while processing the request.",
        //            Data = $"ex{{0}}: {ex.Message}, ex{{1}}: {ex.StackTrace}, ex{{2}}: {ex.InnerException?.Message ?? "N/A"}"
        //        });
        //    }
        //}
        [HttpPost]
        [Route("AddAssetsMaster")]
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

        //[HttpPut]
        //[Route("UpdateAssetsMaster")]
        //public async Task<IActionResult> UpdateAssetsMaster(AssetsMaster assetsMaster)
        //{
        //    Log.Information("UpdateAssetsMaster API called.");

        //    var isUpdated = await _assetsMasterService.UpdateAssetsMaster(assetsMaster);
        //    if (!isUpdated)
        //    {
        //        Log.Warning("Failed to update AssetsMaster with ID {AssetsMasterId}.", assetsMaster.Id);
        //        return BadRequest(new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.AssetsMasterNotUpdated,
        //            Data = false
        //        });
        //    }

        //    Log.Information("AssetsMaster with ID {AssetsMasterId} updated successfully.", assetsMaster.Id);
        //    return Ok(new ApiResponse<bool>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.AssetsMasterUpdated,
        //        Data = true
        //    });
        //}
        [HttpPut]
        [Route("UpdateAssetsMaster")]
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


        //[HttpDelete]
        //[Route("DeleteAssetsMaster")]
        //public async Task<IActionResult> DeleteAssetsMaster(long AssetsMasterId)
        //{
        //    try
        //    {
        //        Log.Information("DeleteAssetsMaster API called for ID {AssetsMasterId}.", AssetsMasterId);

        //        var isDeleted = await _assetsMasterService.DeleteAssetsMaster(AssetsMasterId);
        //        if (!isDeleted)
        //        {
        //            Log.Warning("Failed to delete AssetsMaster with ID {AssetsMasterId}.", AssetsMasterId);
        //            return BadRequest(new ApiResponse<bool>
        //            {
        //                StatusCode = (int)HttpStatusCode.BadRequest,
        //                Message = ApiMessageConstant.AssetsMasterNotDeleted,
        //                Data = false
        //            });
        //        }

        //        Log.Information("AssetsMaster with ID {AssetsMasterId} deleted successfully.", AssetsMasterId);
        //        return Ok(new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.AssetsMasterDeleted,
        //            Data = true
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, "Error occurred in DeleteAssetsMaster for ID {AssetsMasterId}. ex{0}: {Message}, ex{1}: {StackTrace}, ex{2}: {InnerException}",
        //            AssetsMasterId, ex.Message, ex.StackTrace, ex.InnerException?.Message ?? "N/A");

        //        return StatusCode(500, new ApiResponse<string>
        //        {
        //            StatusCode = 500,
        //            Message = "An error occurred while deleting AssetsMaster.",
        //            Data = $"ex{{0}}: {ex.Message}, ex{{1}}: {ex.StackTrace}, ex{{2}}: {ex.InnerException?.Message ?? "N/A"}"
        //        });
        //    }
        //}
        [HttpDelete]
        [Route("DeleteAssetsMaster")]
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
