using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KHRMS.Authorization;
using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
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
    public class ResignationController : Controller
    {
        private readonly IResignationService _resign;
        private readonly IEmployeeAccessService _employeeAccessService;

        public ResignationController(
            IResignationService resignationService,
            IEmployeeAccessService employeeAccessService)
        {
            _resign = resignationService;
            _employeeAccessService = employeeAccessService;
        }

        [HttpGet]
        [Route("GetResignation")]
        public async Task<IActionResult> GetResignation()
        {
            Log.Information("GetResignation API called.");

            var resignations = await _resign.GetAllResignations();
            if (resignations == null || !resignations.Any())
            {
                Log.Information("No Resignation records found.");
                return Ok(new ApiResponse<List<ResignationRequestModel>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.ResignationRequestNotFound,
                    Data = null
                });
            }

            Log.Information("Resignation records found successfully.");
            return Ok(new ApiResponse<List<ResignationRequestModel>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.ResignationRequestFound,
                Data = (await _employeeAccessService.CanAccessEmployeeAsync(0, "RESIGNATION_MANAGE"))
                    ? resignations.ToList()
                    : resignations.Where(r => r.EmployeeId == _employeeAccessService.GetCurrentEmployeeId()).ToList()
            });
        }

        [HttpPost]
        [Route("AddResignation")]
        public async Task<IActionResult> AddResignation(ResignationRequestModel resignation)
        {
            Log.Information("AddResignation API called.");

            var currentEmpId = _employeeAccessService.GetCurrentEmployeeId();
            if (!await _employeeAccessService.CanAccessEmployeeAsync(0, "RESIGNATION_MANAGE") && currentEmpId > 0)
            {
                resignation.EmployeeId = currentEmpId;
            }

            var isAdded = await _resign.AddResignations(resignation);
            if (!isAdded)
            {
                Log.Warning("Failed to add AddResignation.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.ResignationRequestNotFound,
                    Data = false
                });
            }

            Log.Information("AddResignation added successfully.");
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.ResignationRequestFound,
                Data = true
            });
        }

        [HttpPut]
        [Route("UpdateResignation")]
        public async Task<IActionResult> UpdateResignation(Resignation resignation)
        {
            Log.Information("UpdateResignation API called.");

            if (!await _employeeAccessService.CanAccessEmployeeAsync(resignation.EmployeeId, "RESIGNATION_MANAGE"))
            {
                return Forbid();
            }

            var isUpdated = await _resign.UpdateResignation(resignation);
            if (!isUpdated)
            {
                Log.Warning("Failed to update Resignation with ID {ResignationId}.", resignation.Id);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.ResignationRequestNotFound,
                    Data = false
                });
            }

            Log.Information("Resignation with ID {ResignationId} updated successfully.", resignation.Id);
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.ResignationRequestUpdated,
                Data = true
            });
        }

        [HttpDelete]
        [Route("DeleteResignation")]
        [RequirePermission("RESIGNATION_MANAGE")]
        public async Task<IActionResult> DeleteResignation(long ResignationId)
        {
            Log.Information("DeleteResignation API called for ID {ResignationId}.", ResignationId);

            var isDeleted = await _resign.DeleteResignation(ResignationId);
            if (!isDeleted)
            {
                Log.Warning("Failed to delete Resignation with ID {ResignationId}.", ResignationId);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.ResignationRequestNotFound,
                    Data = false
                });
            }

            Log.Information("Resignation with ID {ResignationId} deleted successfully.", ResignationId);
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.ResignationRequestDeleted,
                Data = true
            });
        }

        [HttpPut]
        [Route("ApproveOrRejectResignation")]
        [RequirePermission("RESIGNATION_MANAGE")]
        public async Task<IActionResult> ApproveOrRejectResignation(Resignation resignation)
        {
            Log.Information("ApproveOrRejectResignation API called.");

            var isUpdated = await _resign.ApproveOrRejectResignation(resignation);
            if (!isUpdated)
            {
                Log.Warning("Failed to update Resignation with ID {ResignationId}.", resignation.Id);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.ResignationRequestNotFound,
                    Data = false
                });
            }

            Log.Information("Resignation with ID {ResignationId} updated successfully.", resignation.Id);
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.ResignationRequestUpdated,
                Data = true
            });
        }

        [HttpPost("withdraw/{id}")]
        public async Task<IActionResult> WithdrawResignation(long id)
        {
            Log.Information("WithdrawResignation API called for ID {ResignationId}.", id);
            try
            {
                var success = await _resign.WithdrawResignationAsync(id);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = success ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    Message = success ? "Resignation request withdrawn successfully." : "Failed to withdraw resignation.",
                    Data = success
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = false
                });
            }
        }

        [HttpPost("manager-review/{id}")]
        public async Task<IActionResult> ManagerReview(long id, [FromBody] ManagerResignationReviewDto dto)
        {
            Log.Information("ManagerReview API called for ID {ResignationId}.", id);
            try
            {
                var success = await _resign.ManagerReviewResignationAsync(id, dto);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = success ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    Message = success ? (dto.Accept ? "Resignation accepted by manager and forwarded to HR." : "Resignation rejected by manager.") : "Failed to process manager review.",
                    Data = success
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = false
                });
            }
        }

        [HttpPost("hr-approve/{id}")]
        [RequirePermission("RESIGNATION_MANAGE")]
        public async Task<IActionResult> HrApprove(long id, [FromBody] HrResignationApprovalDto dto)
        {
            Log.Information("HrApprove API called for ID {ResignationId}.", id);
            try
            {
                var success = await _resign.HrApproveResignationAsync(id, dto);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = success ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    Message = success ? "Resignation approved by HR, Last Working Day locked, and Offboarding checklist initiated." : "Failed to approve resignation.",
                    Data = success
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = false
                });
            }
        }

        [HttpPost("finalize-exit/{id}")]
        [RequirePermission("RESIGNATION_MANAGE")]
        public async Task<IActionResult> FinalizeExit(long id, [FromBody] FinalizeExitDto dto)
        {
            Log.Information("FinalizeExit API called for ID {ResignationId}.", id);
            try
            {
                var success = await _resign.FinalizeEmployeeExitAsync(id, dto);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = success ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    Message = success ? "Employee marked Exited, login disabled, and pending manager approvals reassigned." : "Failed to finalize employee exit.",
                    Data = success
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = false
                });
            }
        }

        [HttpPost("rehire/{employeeId}")]
        [RequirePermission("RESIGNATION_MANAGE")]
        public async Task<IActionResult> RehireEmployee(long employeeId, [FromBody] RehireEmployeeDto dto)
        {
            Log.Information("RehireEmployee API called for employee {EmployeeId}.", employeeId);
            try
            {
                var success = await _resign.RehireEmployeeAsync(employeeId, dto);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = success ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    Message = success ? "New employee tenure created for rehire. Historical records linked and read-only. Login pointed to new tenure." : "Failed to rehire employee.",
                    Data = success
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = false
                });
            }
        }

        [HttpPut("{id}/fnf-status")]
        [RequirePermission("RESIGNATION_MANAGE")]
        public async Task<IActionResult> UpdateFnFStatus(long id, [FromBody] FnFStatusUpdateDto dto)
        {
            try
            {
                var success = await _resign.UpdateFnFStatusAsync(id, dto);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = success ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    Message = success ? "Full & Final settlement status updated." : "Failed to update FnF status.",
                    Data = success
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = false
                });
            }
        }

        [HttpGet("overdue-exits-count")]
        public async Task<IActionResult> GetOverdueExitsCount()
        {
            var count = await _resign.GetOverdueExitsCountAsync();
            return Ok(new ApiResponse<int>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Overdue exits count retrieved",
                Data = count
            });
        }

        [HttpGet("overdue-exits")]
        public async Task<IActionResult> GetOverdueExits()
        {
            var overdue = await _resign.GetOverdueExitsAsync();
            return Ok(new ApiResponse<IEnumerable<Resignation>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Overdue exits list retrieved",
                Data = overdue
            });
        }
    }
}
