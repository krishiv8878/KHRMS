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
    public class HolidayController(IHolidayService holidayService): ControllerBase
    {
        private readonly IHolidayService _holidayService = holidayService;
        /// <summary>
        /// Get the list of Holidays
        /// </summary>
        /// <returns></returns>

        //[HttpGet]
        //[Route("GetHolidays")]

        //public async Task<IActionResult> GetHolidays()
        //{
        //    var holidays = await _holidayService.GetAllHolidays();
        //    if(holidays == null)
        //    {
        //        return NotFound();
        //    }

        //    var response = new ApiResponse<List<Holiday>>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = holidays.Any() ? ApiMessageConstant.HolidayFound : ApiMessageConstant.NoHolidayFound,
        //        Data = holidays.ToList()
        //    };
        //    return Ok(response);
        //}

        [HttpGet("GetHolidays")]
        public async Task<IActionResult> GetHolidays()
        {
            Log.Information("HolidayController - GetHolidays called.");
            var holidays = await _holidayService.GetAllHolidays();

            if (holidays == null || !holidays.Any())
            {
                Log.Warning("HolidayController - No holidays found.");
                return Ok(new ApiResponse<List<Holiday>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.NoHolidayFound,
                    Data = null
                });
            }

            Log.Information("HolidayController - {Count} holidays found.", holidays.Count());
            return Ok(new ApiResponse<List<Holiday>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.HolidayFound,
                Data = holidays.ToList()
            });
        }

        /// <summary>
        /// Add a new Holiday
        /// </summary>
        /// <param name="holiday"></param>
        /// <returns></returns>
        //[HttpPost]
        //[Route("AddHoliday")]
        //public async Task<IActionResult> AddHoliday(Holiday holiday)
        //{
        //    var isHolidayAdded = await _holidayService.CreateHoliday(holiday);
        //    if (isHolidayAdded)
        //    {
        //        // Use the wrapper class to create a consistent response
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.HolidayAdded,
        //            Data = isHolidayAdded
        //        };
        //        return Ok(response);
        //    }
        //    else
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.HolidayNotAdded,
        //            Data = isHolidayAdded
        //        };
        //        return BadRequest(response);
        //    }
        //}
        [HttpPost("AddHoliday")]
        public async Task<IActionResult> AddHoliday([FromBody] Holiday holiday)
        {
            Log.Information("HolidayController - AddHoliday called.");
            var result = await _holidayService.CreateHoliday(holiday);

            if (result)
            {
                Log.Information("HolidayController - Holiday added successfully.");
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.HolidayAdded,
                    Data = true
                });
            }

            Log.Warning("HolidayController - Failed to add holiday.");
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.HolidayNotAdded,
                Data = false
            });
        }

        /// <summary>
        /// Update a existing Holiday
        /// </summary>
        /// <param name="holiday"></param>
        /// <returns></returns>
        //[HttpPut]
        //[Route("UpdateHoliday")]
        //public async Task<IActionResult> UpdateHoliday(Holiday holiday)
        //{
        //    var isHolidayEdited = await _holidayService.UpdateHoliday(holiday);
        //    if (isHolidayEdited)
        //    {
        //        // Use the wrapper class to create a consistent response
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.HolidayUpdated,
        //            Data = isHolidayEdited
        //        };
        //        return Ok(response);
        //    }
        //    else
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.HolidayNotUpdated,
        //            Data = isHolidayEdited
        //        };
        //        return BadRequest(response);
        //    }
        //}
        [HttpPut("UpdateHoliday")]
        public async Task<IActionResult> UpdateHoliday([FromBody] Holiday holiday)
        {
            Log.Information("HolidayController - UpdateHoliday called.");
            var result = await _holidayService.UpdateHoliday(holiday);

            if (result)
            {
                Log.Information("HolidayController - Holiday updated successfully.");
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.HolidayUpdated,
                    Data = true
                });
            }

            Log.Warning("HolidayController - Failed to update holiday.");
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.HolidayNotUpdated,
                Data = false
            });
        }
        /// <summary>
        /// Delete existing holiday
        /// </summary>
        /// <param name="holiday"></param>
        /// <returns></returns>
        
        //public async Task<IActionResult> DeleteHoliday(long holidayId)
        //{
        //    var isHolidayDeleted = await _holidayService.DeleteHoliday(holidayId);
        //    if (isHolidayDeleted)
        //    {
        //        // Use the wrapper class to create a consistent response
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.HolidayDeleted,
        //            Data = isHolidayDeleted
        //        };
        //        return Ok(response);
        //    }
        //    else
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.HolidayNotDeleted,
        //            Data = isHolidayDeleted
        //        };
        //        return BadRequest(response);
        //    }
        //}
        [HttpDelete]
        [Route("DeleteHoliday")]
        public async Task<IActionResult> DeleteHoliday(long holidayId)
        {
            Log.Information("HolidayController - DeleteHoliday called with ID: {Id}", holidayId);
            var result = await _holidayService.DeleteHoliday(holidayId);

            if (result)
            {
                Log.Information("HolidayController - Holiday deleted successfully for ID: {Id}", holidayId);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.HolidayDeleted,
                    Data = true
                });
            }

            Log.Warning("HolidayController - Failed to delete holiday for ID: {Id}", holidayId);
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.HolidayNotDeleted,
                Data = false
            });
        }
    }
}
