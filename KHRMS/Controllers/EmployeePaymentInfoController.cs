using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;

namespace KHRMS
{
    /// <summary>
    /// API Controller for managing Employee Payment Information.
    /// Provides endpoints to Create, Read, Update, and Delete employee payment records.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]

    public class EmployeePaymentInfoController(IEmployeePaymentInfoService employeePaymentInfoService) : ControllerBase

    {
        private readonly IEmployeePaymentInfoService _employeePaymentInfoService = employeePaymentInfoService;

        //[HttpGet("GetAllPaymentInfo")]
        //public async Task<IActionResult> GetAll()
        //{
        //    var entities = await _employeePaymentInfoService.GetAllAsync();
        //    return Ok(new ApiResponse<IEnumerable<EmployeePaymentInfo>>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.AllEmployeePaymentRequestsFound,
        //        Data = entities
        //    });
        //}

        [HttpGet("GetAllPaymentInfo")]
        public async Task<IActionResult> GetAll()
        {
            Log.Information("EmployeePaymentInfoController - GetAllPaymentInfo called.");

            var entities = await _employeePaymentInfoService.GetAllAsync();

            if (entities == null || !entities.Any())
            {
                Log.Warning("EmployeePaymentInfoController - No payment info records found.");
                return NotFound(new ApiResponse<IEnumerable<EmployeePaymentInfo>>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ApiMessageConstant.EmployeePaymentRequestsNotFound,
                    Data = null
                });
            }

            Log.Information("EmployeePaymentInfoController - {Count} payment info records found.", entities.Count());
            return Ok(new ApiResponse<IEnumerable<EmployeePaymentInfo>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AllEmployeePaymentRequestsFound,
                Data = entities
            });
        }


        /// <summary>
        /// Retrieves employee payment information by ID.
        /// </summary>
        /// <param name="id">Employee Payment Info ID</param>

        //[HttpGet("GetPaymentInfoProfileById/{id}")]
        //public async Task<IActionResult> GetById(long id)
        //{
        //    var entity = await _employeePaymentInfoService.GetByIdAsync(id);
        //    if (entity == null)
        //    {
        //        return NotFound(new ApiResponse<EmployeePaymentInfo>
        //        {
        //            StatusCode = (int)HttpStatusCode.NotFound,
        //            Message = ApiMessageConstant.EmployeePaymentRequestsNotFound,
        //            Data = null
        //        });
        //    }
        //    return Ok(new ApiResponse<EmployeePaymentInfo>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.EmployeePaymentRequestsFound,
        //        Data = entity
        //    });
        //}
        [HttpGet]
        [Route("GetPaymentInfoProfileById")]
        public async Task<IActionResult> GetById(long id)
        {
            Log.Information("EmployeePaymentInfoController - GetPaymentInfoProfileById called with ID: {Id}", id);
            var entity = await _employeePaymentInfoService.GetByIdAsync(id);
            if (entity == null)
            {
                Log.Warning("EmployeePaymentInfoController - No record found for ID: {Id}", id);
                return NotFound(new ApiResponse<EmployeePaymentInfo>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ApiMessageConstant.EmployeePaymentRequestsNotFound,
                    Data = null
                });
            }

            Log.Information("EmployeePaymentInfoController - Record found for ID: {Id}", id);
            return Ok(new ApiResponse<EmployeePaymentInfo>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmployeePaymentRequestsFound,
                Data = entity
            });
        }

        /// <summary>
        /// Creates a new employee payment information record.
        /// </summary>
        /// <param name="entity">Employee Payment Info object</param>

        //[HttpPost("CreatePaymentInfo")]
        //public async Task<IActionResult> Create([FromBody] EmployeePaymentInfo entity)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.InvalidDataofPaymentInfo,
        //            Data = false
        //        });
        //    }
        //    await _employeePaymentInfoService.AddAsync(entity);
        //    return Ok(new ApiResponse<bool>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.EmployeePaymentInfoAdd,
        //        Data = true
        //    });
        //}
        [HttpPost("CreatePaymentInfo")]
        public async Task<IActionResult> Create([FromBody] EmployeePaymentInfo entity)
        {
            Log.Information("EmployeePaymentInfoController - CreatePaymentInfo called.");
            if (!ModelState.IsValid)
            {
                Log.Warning("EmployeePaymentInfoController - Invalid model state for CreatePaymentInfo.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidDataofPaymentInfo,
                    Data = false
                });
            }

            await _employeePaymentInfoService.AddAsync(entity);
            Log.Information("EmployeePaymentInfoController - Payment info created successfully.");

            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmployeePaymentInfoAdd,
                Data = true
            });
        }
        /// <summary>
        /// Updates an existing employee payment information record.
        /// </summary>
        /// <param name="id">Employee Payment Info ID</param>
        /// <param name="entity">Updated Employee Payment Info object</param>

        //[HttpPut("UpdatePaymentInfo/{id}")]
        //public async Task<IActionResult> Update(long id, [FromBody] EmployeePaymentInfo entity)
        //{
        //    if (!ModelState.IsValid || id != entity.Id)
        //    {
        //        return BadRequest(new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.InvalidData,
        //            Data = false
        //        });
        //    }
        //    await _employeePaymentInfoService.UpdateAsync(entity);
        //    return Ok(new ApiResponse<bool>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.PaymenInfoeRequestUpdated,
        //        Data = true
        //    });
        //}
        [HttpPut]
        [Route("UpdatePaymentInfo")]
        public async Task<IActionResult> Update([FromBody] EmployeePaymentInfo entity)
        {
            Log.Information("EmployeePaymentInfoController - UpdatePaymentInfo called for ID: {Id}", entity.Id);

            if (!ModelState.IsValid || entity.Id == null)
            {
                Log.Warning("EmployeePaymentInfoController - Invalid model state or ID Not Found for UpdatePaymentInfo.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidData,
                    Data = false
                });
            }

            await _employeePaymentInfoService.UpdateAsync(entity);
            Log.Information("EmployeePaymentInfoController - Payment info updated successfully for ID: {Id}", entity.Id);

            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.PaymenInfoeRequestUpdated,
                Data = true
            });
        }
        /// <summary>
        /// Deletes an employee payment information record by ID.
        /// </summary>
        /// <param name="id">Employee Payment Info ID</param>
        //[HttpDelete("DeletePaymentInfo/{id}")]
        //public async Task<IActionResult> Delete(long id)
        //{
        //    var paymentinfo = await _employeePaymentInfoService.GetByIdAsync(id);
        //    if (paymentinfo == null)
        //        return NotFound();
        //    await _employeePaymentInfoService.DeleteAsync(id);
        //    return Ok(new ApiResponse<bool>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.PaymentInfoRequestDeleted,
        //        Data = true
        //    });
        //}
        [HttpDelete]
        [Route("DeletePaymentInfo")]
        public async Task<IActionResult> Delete(long id)
        {
            Log.Information("EmployeePaymentInfoController - DeletePaymentInfo called with ID: {Id}", id);

            var paymentinfo = await _employeePaymentInfoService.GetByIdAsync(id);
            if (paymentinfo == null)
            {
                Log.Warning("EmployeePaymentInfoController - Payment info not found for ID: {Id}", id);
                return NotFound(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ApiMessageConstant.EmployeePaymentRequestsNotFound,
                    Data = false
                });
            }

            await _employeePaymentInfoService.DeleteAsync(id);
            Log.Information("EmployeePaymentInfoController - Payment info deleted successfully for ID: {Id}", id);

            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.PaymentInfoRequestDeleted,
                Data = true
            });
        }
    }


}
