using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;

namespace KHRMS
{
    /// <summary>
    /// API Controller for managing Employee Shift Information.
    /// Provides endpoints to Create, Read, Update, and Delete employee payment records.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ShiftController(IShiftService shiftService) : ControllerBase

    {
        public readonly IShiftService _shiftService = shiftService;


        [HttpGet("GetAllShifts")]
        public async Task<IActionResult> GetAllShifts()
        {
            Log.Information("ShiftController - GetAllShifts called.");

            var shifts = await _shiftService.GetAllShiftsAsync();

            Log.Information("ShiftController - {Count} shifts found.", shifts?.Count() ?? 0);

            return Ok(new ApiResponse<IEnumerable<ShiftMaster>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AllEmployeeShiftRecordRequestsFound,
                Data = shifts
            });
        }


        /// <summary>
        /// Retrieves employee shift information by ID.
        /// </summary>
        /// <param name="id">Employee Shift Info ID</param>

        [HttpGet("GetShiftById/{id}")]
        public async Task<IActionResult> GetShiftById(long id)
        {
            Log.Information("ShiftController - GetShiftById called for ID: {Id}", id);

            var shift = await _shiftService.GetShiftByIdAsync(id);

            if (shift == null)
            {
                Log.Warning("ShiftController - Shift not found for ID: {Id}", id);
                return Ok(new ApiResponse<ShiftMaster>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.EmployeeShiftRequestsNotFound,
                    Data = null
                });
            }

            Log.Information("ShiftController - Shift found for ID: {Id}", id);

            return Ok(new ApiResponse<ShiftMaster>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmployeeShiftRequestsFound,
                Data = shift
            });
        }

        /// <summary>
        /// Creates a new employee Shift information record.
        /// </summary>
        /// <param name="shift">Employee Shift Info object</param>


        [HttpPost("CreateShiftType")]
        public async Task<IActionResult> AddShift([FromBody] ShiftMaster shift)
        {
            Log.Information("ShiftController - AddShift called.");

            if (!ModelState.IsValid)
            {
                Log.Warning("ShiftController - Invalid model state.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidDataofShiftInfo,
                    Data = false
                });
            }

            await _shiftService.AddShiftAsync(shift);

            Log.Information("ShiftController - Shift added with ID: {Id}", shift.Id);

            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmployeeShifttInfoAdd,
                Data = true
            });
        }

        /// <summary>
        /// Updates an existing employee Shift information record.
        /// </summary>
        /// <param name="shiftMaster">Updated Employee Shift Info object</param>

        [HttpPut("UpdateShift/{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] ShiftMaster shiftMaster)
        {
            Log.Information("ShiftController - Update called for ID: {Id}", shiftMaster.Id);

            if (shiftMaster.Id == null)
            {
                Log.Warning("ShiftController - ID Not Found. Route ID: {RouteId}, Body ID: {BodyId}", shiftMaster.Id);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "ID Does Not Found!",
                    Data = false
                });
            }

            if (!ModelState.IsValid)
            {
                Log.Warning("ShiftController - Invalid model state.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidData,
                    Data = false
                });
            }

            await _shiftService.UpdateShiftAsync(shiftMaster);

            Log.Information("ShiftController - Shift updated for ID: {Id}", shiftMaster.Id);

            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.ShiftInfoeRequestUpdated,
                Data = true
            });
        }

        /// <summary>
        /// Deletes an employee shift information record by ID.
        /// </summary>
        /// <param name="id">Employee shift Info ID</param>


        [HttpDelete("DeleteShift")]
        public async Task<IActionResult> DeleteShift(long id)
        {
            Log.Information("ShiftController - DeleteShift called for ID: {Id}", id);

            var shift = await _shiftService.GetShiftByIdAsync(id);
            if (shift == null)
            {
                Log.Warning("ShiftController - Shift not found for deletion with ID: {Id}", id);
                return Ok(new ApiResponse<ShiftMaster>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.EmployeeShiftRequestsNotFound,
                    Data = null
                });
            }

            await _shiftService.DeleteShiftAsync(id);

            Log.Information("ShiftController - Shift deleted with ID: {Id}", id);

            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.ShiftInfoRequestDeleted,
                Data = true
            });
        }

    }
}


