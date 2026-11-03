using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using KHRMS.Authorization;
using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using KHRMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace KHRMS
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceLockController : ControllerBase
    {
        private readonly IAttendanceLockService _lockService;
        private readonly IUserContextService _userContext;

        public AttendanceLockController(IAttendanceLockService lockService, IUserContextService userContext)
        {
            _lockService = lockService;
            _userContext = userContext;
        }

        [HttpGet("GetAllLocks")]
        [RequirePermission("ATTENDANCE_VIEW_ALL")]
        public async Task<IActionResult> GetAllLocks()
        {
            Log.Information("AttendanceLockController - GetAllLocks called.");
            var locks = await _lockService.GetAllLocksAsync();

            return Ok(new ApiResponse<IEnumerable<AttendanceLockDTO>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Attendance lock records retrieved successfully.",
                Data = locks
            });
        }

        [HttpGet("GetLockForMonth")]
        [RequirePermission("ATTENDANCE_VIEW_ALL")]
        public async Task<IActionResult> GetLockForMonth([FromQuery] int month, [FromQuery] int year)
        {
            Log.Information("AttendanceLockController - GetLockForMonth called for {Month}/{Year}.", month, year);
            var lockRecord = await _lockService.GetLockForMonthAsync(month, year);

            return Ok(new ApiResponse<AttendanceLockDTO?>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = lockRecord != null ? "Attendance lock found." : "Attendance lock not found.",
                Data = lockRecord
            });
        }

        [HttpPost("LockMonth")]
        [RequirePermission("ATTENDANCE_LOCK")]
        public async Task<IActionResult> LockMonth([FromBody] LockMonthRequest request)
        {
            Log.Information("AttendanceLockController - LockMonth called for {Month}/{Year}.", request.Month, request.Year);
            long currentEmpId = _userContext.GetCurrentEmployeeId();

            try
            {
                var result = await _lockService.LockMonthAsync(request.Month, request.Year, currentEmpId);
                return Ok(new ApiResponse<AttendanceLockDTO>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = $"Attendance for {request.Month:D2}/{request.Year} has been locked successfully.",
                    Data = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<string>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ApiResponse<string>
                {
                    StatusCode = (int)HttpStatusCode.Conflict,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        [HttpPost("UnlockMonth")]
        [RequirePermission("ATTENDANCE_LOCK")]
        public async Task<IActionResult> UnlockMonth([FromBody] UnlockMonthRequest request)
        {
            Log.Information("AttendanceLockController - UnlockMonth called for {Month}/{Year}.", request.Month, request.Year);
            long currentEmpId = _userContext.GetCurrentEmployeeId();

            try
            {
                var result = await _lockService.UnlockMonthAsync(request.Month, request.Year, currentEmpId, request.Reason);
                return Ok(new ApiResponse<AttendanceLockDTO>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = $"Attendance for {request.Month:D2}/{request.Year} has been unlocked successfully.",
                    Data = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<string>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ApiResponse<string>
                {
                    StatusCode = (int)HttpStatusCode.Conflict,
                    Message = ex.Message,
                    Data = null
                });
            }
        }
    }
}
