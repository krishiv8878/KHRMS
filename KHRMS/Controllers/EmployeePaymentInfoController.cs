using KHRMS.Authorization;
using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using KHRMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;

namespace KHRMS
{
    public class EmployeePaymentInfoResponseDto
    {
        public long Id { get; set; }
        public long EmployeeId { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string IFSCCode { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string? NameOnAccount { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    /// <summary>
    /// API Controller for managing Employee Payment Information.
    /// Provides endpoints to Create, Read, Update, and Delete employee payment records.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeePaymentInfoController(
        IEmployeePaymentInfoService employeePaymentInfoService,
        IEmployeeAccessService employeeAccessService) : ControllerBase
    {
        private readonly IEmployeePaymentInfoService _employeePaymentInfoService = employeePaymentInfoService;
        private readonly IEmployeeAccessService _employeeAccessService = employeeAccessService;


        [HttpGet("GetAllPaymentInfo")]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            Log.Information("EmployeePaymentInfoController - GetAllPaymentInfo called.");

            long currentEmpId = _employeeAccessService.GetCurrentEmployeeId();
            bool isAdminOrHr = User.IsInRole("Admin") ||
                               User.IsInRole("System Admin") ||
                               User.IsInRole("HR") ||
                               User.IsInRole("HR Operations") ||
                               User.IsInRole("HR Manager") ||
                               User.IsInRole("HR Admin") ||
                               User.IsInRole("Human Resources");

            var isPrivileged = isAdminOrHr ||
                               await _employeeAccessService.CanAccessEmployeeAsync(0, "PAYROLL_VIEW_ALL") ||
                               await _employeeAccessService.CanAccessEmployeeAsync(0, "EMPLOYEE_PAYMENT_MANAGE") ||
                               await _employeeAccessService.CanAccessEmployeeAsync(0, "PAYROLL_PROCESS") ||
                               await _employeeAccessService.CanAccessEmployeeAsync(0, "PAYROLL_MANAGE");

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
            var canViewRaw = await _employeeAccessService.CanAccessEmployeeAsync(0, "SALARY_VIEW_ALL") ||
                             await _employeeAccessService.CanAccessEmployeeAsync(0, "EMPLOYEE_PAYMENT_MANAGE") ||
                             await _employeeAccessService.CanAccessEmployeeAsync(0, "PAYROLL_VIEW_ALL");

            var resultData = entities.Select(e => ApplyMasking(e, canViewRaw || e.EmployeeId == currentEmpId)).ToList();

            return Ok(new ApiResponse<IEnumerable<EmployeePaymentInfoResponseDto>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AllEmployeePaymentRequestsFound,
                Data = resultData
            });
        }

        /// <summary>
        /// Retrieves employee payment information by Employee ID.
        /// </summary>
        [HttpGet("GetPaymentInfoByEmployeeId/{employeeId?}")]
        public async Task<IActionResult> GetPaymentInfoByEmployeeId(long? employeeId)
        {
            Log.Information("EmployeePaymentInfoController - GetPaymentInfoByEmployeeId called with EmployeeId: {EmployeeId}", employeeId);

            long currentEmpId = _employeeAccessService.GetCurrentEmployeeId();
            long targetEmpId = employeeId.HasValue && employeeId.Value > 0 ? employeeId.Value : currentEmpId;

            if (targetEmpId <= 0)
            {
                return BadRequest(new ApiResponse<EmployeePaymentInfoResponseDto>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Employee ID is required",
                    Data = null
                });
            }

            // Enforce central authorization rule (Self, Team, or Privileged)
            if (!await _employeeAccessService.CanAccessEmployeeAsync(targetEmpId, "PAYROLL_VIEW_ALL") &&
                !await _employeeAccessService.CanAccessEmployeeAsync(targetEmpId, "EMPLOYEE_PAYMENT_MANAGE") &&
                !await _employeeAccessService.CanAccessEmployeeAsync(targetEmpId, "PAYROLL_MANAGE"))
            {
                Log.Warning("EmployeePaymentInfoController - Unauthorized access attempt by {CurrentEmpId} for {TargetEmpId}", currentEmpId, targetEmpId);
                return Forbid();
            }

            var entity = await _employeePaymentInfoService.GetByEmployeeIdAsync(targetEmpId);
            if (entity == null)
            {
                return Ok(new ApiResponse<EmployeePaymentInfoResponseDto>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.EmployeePaymentRequestsNotFound,
                    Data = null
                });
            }

            var canViewRaw = (currentEmpId == targetEmpId) ||
                             await _employeeAccessService.CanAccessEmployeeAsync(0, "SALARY_VIEW_ALL") ||
                             await _employeeAccessService.CanAccessEmployeeAsync(0, "EMPLOYEE_PAYMENT_MANAGE") ||
                             await _employeeAccessService.CanAccessEmployeeAsync(0, "PAYROLL_VIEW_ALL");

            return Ok(new ApiResponse<EmployeePaymentInfoResponseDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmployeePaymentRequestsFound,
                Data = ApplyMasking(entity, canViewRaw)
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
                return Ok(new ApiResponse<EmployeePaymentInfoResponseDto>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.EmployeePaymentRequestsNotFound,
                    Data = null
                });
            }

            long currentEmpId = _employeeAccessService.GetCurrentEmployeeId();
            var canAccess = await _employeeAccessService.CanAccessEmployeeAsync(entity.EmployeeId, "PAYROLL_VIEW_ALL") ||
                            await _employeeAccessService.CanAccessEmployeeAsync(entity.EmployeeId, "EMPLOYEE_PAYMENT_MANAGE") ||
                            await _employeeAccessService.CanAccessEmployeeAsync(entity.EmployeeId, "PAYROLL_MANAGE");

            if (!canAccess && (currentEmpId <= 0 || entity.EmployeeId != currentEmpId))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new ApiResponse<EmployeePaymentInfoResponseDto>
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = "Access Denied: You do not have permission to view another employee's banking details.",
                    Data = null
                });
            }

            var canViewRaw = (currentEmpId == entity.EmployeeId) ||
                             await _employeeAccessService.CanAccessEmployeeAsync(0, "SALARY_VIEW_ALL") ||
                             await _employeeAccessService.CanAccessEmployeeAsync(0, "EMPLOYEE_PAYMENT_MANAGE") ||
                             await _employeeAccessService.CanAccessEmployeeAsync(0, "PAYROLL_VIEW_ALL");

            Log.Information("EmployeePaymentInfoController - Record found for ID: {Id}", id);
            return Ok(new ApiResponse<EmployeePaymentInfoResponseDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmployeePaymentRequestsFound,
                Data = ApplyMasking(entity, canViewRaw)
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

            long currentEmpId = _employeeAccessService.GetCurrentEmployeeId();
            long targetEmpId = entity.EmployeeId > 0 ? entity.EmployeeId : currentEmpId;

            if (entity.Id > 0)
            {
                var existing = await _employeePaymentInfoService.GetByIdAsync(entity.Id);
                if (existing != null)
                {
                    targetEmpId = existing.EmployeeId;
                }
            }

            if (!await _employeeAccessService.CanAccessEmployeeAsync(targetEmpId, "PAYROLL_MANAGE"))
            {
                Log.Warning("EmployeePaymentInfoController - Unauthorized UpdatePaymentInfo attempt by {CurrentEmpId} for {TargetEmpId}", currentEmpId, targetEmpId);
                return Forbid();
            }

            entity.EmployeeId = targetEmpId;

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

            long currentEmpId = _employeeAccessService.GetCurrentEmployeeId();
            long targetEmpId = entity.EmployeeId > 0 ? entity.EmployeeId : currentEmpId;

            if (entity.Id > 0)
            {
                var existing = await _employeePaymentInfoService.GetByIdAsync(entity.Id);
                if (existing != null)
                {
                    targetEmpId = existing.EmployeeId;
                }
            }

            if (!await _employeeAccessService.CanAccessEmployeeAsync(targetEmpId, "PAYROLL_MANAGE"))
            {
                Log.Warning("EmployeePaymentInfoController - Unauthorized UpdatePaymentInfo attempt by {CurrentEmpId} for {TargetEmpId}", currentEmpId, targetEmpId);
                return Forbid();
            }

            entity.EmployeeId = targetEmpId;

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
        [RequirePermission("EMPLOYEE_PAYMENT_MANAGE")]
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

        private static EmployeePaymentInfoResponseDto ApplyMasking(EmployeePaymentInfo item, bool canViewRaw)
        {
            if (item == null) return null!;
            return new EmployeePaymentInfoResponseDto
            {
                Id = item.Id,
                EmployeeId = item.EmployeeId,
                BankName = item.BankName,
                IFSCCode = (canViewRaw ? item.IFSCCode : FieldMaskingHelper.MaskIfsc(item.IFSCCode)) ?? string.Empty,
                AccountNumber = (canViewRaw ? item.AccountNumber : FieldMaskingHelper.MaskAccountNumber(item.AccountNumber)) ?? string.Empty,
                NameOnAccount = item.NameOnAccount,
                IsActive = item.IsActive,
                IsDeleted = item.IsDeleted,
                CreatedDate = item.CreatedDate,
                UpdatedDate = item.UpdatedDate
            };
        }
    }


}
