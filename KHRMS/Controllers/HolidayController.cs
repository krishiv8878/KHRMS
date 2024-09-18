﻿using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet]
        [Route("GetHolidays")]

        public async Task<IActionResult> GetHolidays()
        {
            var holidays = await _holidayService.GetAllHolidays();
            if(holidays == null)
            {
                return NotFound();
            }

            var response = new ApiResponse<List<Holiday>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = holidays.Any() ? ApiMessageConstant.HolidayFound : ApiMessageConstant.NoHolidayFound,
                Data = holidays.ToList()
            };
            return Ok(response);
        }

        /// <summary>
        /// Add a new Holiday
        /// </summary>
        /// <param name="holiday"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddHoliday")]
        public async Task<IActionResult> AddHoliday(Holiday holiday)
        {
            var isHolidayAdded = await _holidayService.CreateHoliday(holiday);
            if (isHolidayAdded)
            {
                // Use the wrapper class to create a consistent response
                var response = new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.HolidayAdded,
                    Data = isHolidayAdded
                };
                return Ok(response);
            }
            else
            {
                var response = new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.HolidayNotAdded,
                    Data = isHolidayAdded
                };
                return BadRequest(response);
            }
        }

        /// <summary>
        /// Update a existing Holiday
        /// </summary>
        /// <param name="holiday"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("UpdateHoliday")]
        public async Task<IActionResult> UpdateHoliday(Holiday holiday)
        {
            var isHolidayEdited = await _holidayService.UpdateHoliday(holiday);
            if (isHolidayEdited)
            {
                // Use the wrapper class to create a consistent response
                var response = new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.HolidayUpdated,
                    Data = isHolidayEdited
                };
                return Ok(response);
            }
            else
            {
                var response = new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.HolidayNotUpdated,
                    Data = isHolidayEdited
                };
                return BadRequest(response);
            }
        }
        /// <summary>
        /// Delete existing holiday
        /// </summary>
        /// <param name="holiday"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("DeleteHoliday")]
        public async Task<IActionResult> DeleteHoliday(long holidayId)
        {
            var isHolidayDeleted = await _holidayService.DeleteHoliday(holidayId);
            if (isHolidayDeleted)
            {
                // Use the wrapper class to create a consistent response
                var response = new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.HolidayDeleted,
                    Data = isHolidayDeleted
                };
                return Ok(response);
            }
            else
            {
                var response = new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.HolidayNotDeleted,
                    Data = isHolidayDeleted
                };
                return BadRequest(response);
            }
        }

    }
}
