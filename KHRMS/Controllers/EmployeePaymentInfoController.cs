using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;

namespace KHRMS
{
    /// <summary>
    /// API Controller for managing Employee Payment Information.
    /// Provides endpoints to Create, Read, Update, and Delete employee payment records.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]

    public class EmployeePaymentInfoController(IEmployeePaymentInfoService employeePaymentInfoService) : ControllerBase

    {
        private readonly IEmployeePaymentInfoService _employeePaymentInfoService = employeePaymentInfoService;


        [HttpGet("GetAllPaymentInfo")]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            Log.Information("EmployeePaymentInfoController - GetAllPaymentInfo called.");

            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("System Admin") || User.IsInRole("HR") || User.IsInRole("HR Operations");

            long currentEmpId = 0;
            var claimVal = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrEmpty(claimVal) && long.TryParse(claimVal, out long parsedId))
            {
                currentEmpId = parsedId;
            }

            IEnumerable<EmployeePaymentInfo> entities;
            if (isPrivileged)
            {
                entities = await _employeePaymentInfoService.GetAllAsync();
            }
            else if (currentEmpId > 0)
            {
                var myInfo = await _employeePaymentInfoService.GetByEmployeeIdAsync(currentEmpId);
                entities = myInfo != null ? new List<EmployeePaymentInfo> { myInfo } : Enumerable.Empty<EmployeePaymentInfo>();
            }
            else
            {
                entities = Enumerable.Empty<EmployeePaymentInfo>();
            }

            if (entities == null || !entities.Any())
            {
                Log.Warning("EmployeePaymentInfoController - No payment info records found.");
                return Ok(new ApiResponse<IEnumerable<EmployeePaymentInfo>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
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
        /// Retrieves employee payment information by Employee ID.
        /// </summary>
        [HttpGet("GetPaymentInfoByEmployeeId/{employeeId?}")]
        public async Task<IActionResult> GetPaymentInfoByEmployeeId(long? employeeId)
        {
            Log.Information("EmployeePaymentInfoController - GetPaymentInfoByEmployeeId called with EmployeeId: {EmployeeId}", employeeId);

            long targetEmpId = employeeId ?? 0;
            if (targetEmpId <= 0)
            {
                var claimVal = User.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(claimVal) && long.TryParse(claimVal, out long parsedClaimId))
                {
                    targetEmpId = parsedClaimId;
                }
            }

            if (targetEmpId <= 0)
            {
                return BadRequest(new ApiResponse<EmployeePaymentInfo>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Employee ID is required",
                    Data = null
                });
            }

            var entity = await _employeePaymentInfoService.GetByEmployeeIdAsync(targetEmpId);
            if (entity == null)
            {
                return Ok(new ApiResponse<EmployeePaymentInfo>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.EmployeePaymentRequestsNotFound,
                    Data = null
                });
            }

            return Ok(new ApiResponse<EmployeePaymentInfo>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmployeePaymentRequestsFound,
                Data = entity
            });
        }

        /// <summary>
        /// Retrieves employee payment information by ID.
        /// </summary>
        /// <param name="id">Employee Payment Info ID</param>
        [HttpGet]
        [Route("GetPaymentInfoProfileById")]
        public async Task<IActionResult> GetById(long id)
        {
            Log.Information("EmployeePaymentInfoController - GetPaymentInfoProfileById called with ID: {Id}", id);
            var entity = await _employeePaymentInfoService.GetByIdAsync(id);
            if (entity == null)
            {
                Log.Warning("EmployeePaymentInfoController - No record found for ID: {Id}", id);
                return Ok(new ApiResponse<EmployeePaymentInfo>
                {
                    StatusCode = (int)HttpStatusCode.OK,
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

        [HttpPost("CreatePaymentInfo")]
        public async Task<IActionResult> Create([FromBody] EmployeePaymentInfo entity)
        {
            Log.Information("EmployeePaymentInfoController - CreatePaymentInfo called.");

            if (entity == null)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidDataofPaymentInfo,
                    Data = false
                });
            }

            if (entity.EmployeeId <= 0)
            {
                var claimVal = User.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(claimVal) && long.TryParse(claimVal, out long parsedId))
                {
                    entity.EmployeeId = parsedId;
                }
            }

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

            try
            {
                await _employeePaymentInfoService.AddAsync(entity);
                Log.Information("EmployeePaymentInfoController - Payment info created successfully.");

                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.EmployeePaymentInfoAdd,
                    Data = true
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "EmployeePaymentInfoController - Error creating payment info");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message,
                    Data = false
                });
            }
        }
        /// <summary>
        /// Updates an existing employee payment information record.
        /// </summary>
        /// <param name="id">Employee Payment Info ID</param>
        /// <param name="entity">Updated Employee Payment Info object</param>


        [HttpPut]
        [Route("UpdatePaymentInfo")]
        public async Task<IActionResult> Update([FromBody] EmployeePaymentInfo entity)
        {
            Log.Information("EmployeePaymentInfoController - UpdatePaymentInfo called for ID: {Id}", entity?.Id);

            if (entity == null)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidData,
                    Data = false
                });
            }

            if (entity.EmployeeId <= 0)
            {
                var claimVal = User.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(claimVal) && long.TryParse(claimVal, out long parsedId))
                {
                    entity.EmployeeId = parsedId;
                }
            }

            try
            {
                await _employeePaymentInfoService.UpdateAsync(entity);
                Log.Information("EmployeePaymentInfoController - Payment info updated successfully for ID: {Id}", entity.Id);

                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.PaymenInfoeRequestUpdated,
                    Data = true
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "EmployeePaymentInfoController - Error updating payment info");
                var msg = ex.InnerException != null ? $"{ex.Message} --> {ex.InnerException.Message}" : ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = msg,
                    Data = false
                });
            }
        }
        /// <summary>
        /// Deletes an employee payment information record by ID.
        /// </summary>
        /// <param name="id">Employee Payment Info ID</param>

        [HttpDelete]
        [Route("DeletePaymentInfo")]
        [Authorize(Roles = "Admin,System Admin,HR,HR Operations")]
        public async Task<IActionResult> Delete(long id)
        {
            Log.Information("EmployeePaymentInfoController - DeletePaymentInfo called with ID: {Id}", id);

            var paymentinfo = await _employeePaymentInfoService.GetByIdAsync(id);
            if (paymentinfo == null)
            {
                Log.Warning("EmployeePaymentInfoController - Payment info not found for ID: {Id}", id);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
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
