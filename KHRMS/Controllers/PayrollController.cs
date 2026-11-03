using System.Net;
using KHRMS.Authorization;
using KHRMS.Core.Models;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KHRMS.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PayrollController : ControllerBase
    {
        private readonly IPayrollService _payrollService;
        private readonly IUserContextService _userContext;

        public PayrollController(IPayrollService payrollService, IUserContextService userContext)
        {
            _payrollService = payrollService;
            _userContext = userContext;
        }

        #region 1. Salary Components & Structure

        [HttpGet("components")]
        public async Task<IActionResult> GetComponents()
        {
            var components = await _payrollService.GetAllComponentsAsync();
            return Ok(new ApiResponse<IEnumerable<SalaryComponent>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Salary components retrieved successfully",
                Data = components
            });
        }

        [HttpPost("components")]
        [RequirePermission("PAYROLL_CONFIG_RULES")]
        public async Task<IActionResult> SaveComponent([FromBody] SalaryComponent component)
        {
            if (component == null) return BadRequest("Invalid component data");

            SalaryComponent result;
            if (component.Id > 0)
                result = await _payrollService.UpdateSalaryComponentAsync(component);
            else
                result = await _payrollService.AddSalaryComponentAsync(component);

            return Ok(new ApiResponse<SalaryComponent>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Salary component saved successfully",
                Data = result
            });
        }

        [HttpDelete("components/{id}")]
        [RequirePermission("PAYROLL_CONFIG_RULES")]
        public async Task<IActionResult> DeleteComponent(long id)
        {
            var success = await _payrollService.DeleteSalaryComponentAsync(id);
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = success ? "Component removed" : "Component not found",
                Data = success
            });
        }

        [HttpPost("calculate-ctc")]
        public IActionResult CalculateCtc([FromBody] SalaryCalculationRequest request)
        {
            if (request == null) return BadRequest("Invalid request");
            var result = _payrollService.CalculateCtcBreakdown(request);
            return Ok(new ApiResponse<SalaryCalculationResult>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "CTC calculated successfully",
                Data = result
            });
        }

        [HttpGet("salary-assignment/{employeeId?}")]
        public async Task<IActionResult> GetSalaryAssignment(long? employeeId)
        {
            long currentEmpId = _userContext.GetCurrentEmployeeId();
            long targetId = employeeId.HasValue && employeeId.Value > 0 ? employeeId.Value : currentEmpId;

            bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR() || User.IsInRole("Admin") || User.IsInRole("HR") || await _userContext.HasPermissionAsync("PAYROLL_VIEW_ALL");

            if (!isPrivileged && targetId != currentEmpId)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new ApiResponse<EmployeeSalaryAssignment?>
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = "Access Denied: You do not have permission to view another employee's salary structure.",
                    Data = null
                });
            }

            var assignment = await _payrollService.GetEmployeeSalaryAssignmentAsync(targetId);
            return Ok(new ApiResponse<EmployeeSalaryAssignment?>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = assignment != null ? "Salary assignment found" : "No salary assignment found",
                Data = assignment
            });
        }

        [HttpPost("salary-assignment")]
        [RequirePermission("PAYROLL_CONFIG_RULES")]
        public async Task<IActionResult> AssignSalary([FromBody] AssignSalaryRequest request)
        {
            if (request == null || request.EmployeeId <= 0) return BadRequest("Valid EmployeeId required");
            var result = await _payrollService.AssignSalaryAsync(request);
            return Ok(new ApiResponse<EmployeeSalaryAssignment>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Salary assigned successfully",
                Data = result
            });
        }

        [HttpGet("salary-assignments-all")]
        [RequirePermission("PAYROLL_VIEW_ALL")]
        public async Task<IActionResult> GetAllSalaryAssignments()
        {
            var assignments = await _payrollService.GetAllSalaryAssignmentsAsync();
            return Ok(new ApiResponse<IEnumerable<EmployeeSalaryAssignment>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "All salary assignments retrieved",
                Data = assignments
            });
        }

        [HttpGet("salary-assignment/history/{employeeId}")]
        public async Task<IActionResult> GetSalaryHistory(long employeeId)
        {
            long currentEmpId = _userContext.GetCurrentEmployeeId();
            bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR() || User.IsInRole("Admin") || User.IsInRole("HR") || await _userContext.HasPermissionAsync("PAYROLL_VIEW_ALL");

            if (!isPrivileged && employeeId != currentEmpId)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new ApiResponse<IEnumerable<EmployeeSalaryAssignment>?>
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = "Access Denied: You do not have permission to view another employee's salary history.",
                    Data = null
                });
            }

            var history = await _payrollService.GetEmployeeSalaryHistoryAsync(employeeId);
            return Ok(new ApiResponse<IEnumerable<EmployeeSalaryAssignment>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Salary revision history retrieved successfully",
                Data = history
            });
        }

        #endregion

        #region 2. Pay Run Engine

        [HttpGet("payruns")]
        [RequirePermission("PAYROLL_VIEW_ALL")]
        public async Task<IActionResult> GetPayRuns()
        {
            var runs = await _payrollService.GetAllPayRunsAsync();
            return Ok(new ApiResponse<IEnumerable<PayRun>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Pay runs retrieved",
                Data = runs
            });
        }

        [HttpGet("payrun/{id}")]
        [RequirePermission("PAYROLL_VIEW_ALL")]
        public async Task<IActionResult> GetPayRun(long id)
        {
            var run = await _payrollService.GetPayRunByIdAsync(id);
            if (run == null) return NotFound(new ApiResponse<object> { StatusCode = 404, Message = "Pay run not found" });

            var details = await _payrollService.GetPayRunDetailsAsync(id);
            return Ok(new ApiResponse<object>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Pay run retrieved",
                Data = new { PayRun = run, Employees = details }
            });
        }

        [HttpPost("payrun/create")]
        [RequirePermission("PAYROLL_PROCESS")]
        public async Task<IActionResult> CreatePayRun([FromBody] CreatePayRunRequest request)
        {
            if (request == null || request.Month < 1 || request.Month > 12 || request.Year < 2000)
                return BadRequest(new ApiResponse<object> { StatusCode = 400, Message = "Invalid pay run month and year" });

            try
            {
                var result = await _payrollService.CreatePayRunAsync(request);
                    return Ok(new ApiResponse<PayRun>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = $"Pay run created for {request.Month}/{request.Year}",
                    Data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message
                });
            }
        }

        [HttpDelete("payrun/{id}")]
        [RequirePermission("PAYROLL_PROCESS")]
        public async Task<IActionResult> DeletePayRun(long id)
        {
            try
            {
                var success = await _payrollService.DeletePayRunAsync(id);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = success ? "Pay run discarded successfully" : "Pay run not found",
                    Data = success
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("payrun/status")]
        [RequirePermission("PAYROLL_PROCESS")]
        public async Task<IActionResult> UpdatePayRunStatus([FromBody] UpdatePayRunStatusRequest request)
        {
            if (request == null || request.PayRunId <= 0) return BadRequest("Valid PayRunId required");
            try
            {
                var result = await _payrollService.UpdatePayRunStatusAsync(request);
                return Ok(new ApiResponse<PayRun>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = $"Pay run status updated to {request.Status}",
                    Data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("payrun/{id}/lock")]
        [RequirePermission("PAYROLL_PROCESS")]
        public async Task<IActionResult> LockPayRun(long id)
        {
            try
            {
                long currentEmpId = _userContext.GetCurrentEmployeeId();
                var success = await _payrollService.LockPayRunAsync(id, currentEmpId);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Pay run locked successfully",
                    Data = success
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("payrun/{id}/recalculate")]
        [RequirePermission("PAYROLL_PROCESS")]
        public async Task<IActionResult> RecalculatePayRun(long id)
        {
            try
            {
                var success = await _payrollService.RecalculatePayRunAsync(id);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Pay run recalculated successfully",
                    Data = success
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("payrun/detail/hold")]
        [RequirePermission("PAYROLL_PROCESS")]
        public async Task<IActionResult> ToggleHoldEmployeeDetail([FromBody] ToggleHoldRequest request)
        {
            if (request == null || request.DetailId <= 0) return BadRequest("Valid DetailId required");
            try
            {
                var success = await _payrollService.ToggleHoldEmployeeDetailAsync(request.DetailId, request.IsOnHold, request.Reason);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = request.IsOnHold ? "Employee salary placed on hold" : "Employee salary hold released",
                    Data = success
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("payrun/detail/adjustment")]
        [RequirePermission("PAYROLL_PROCESS")]
        public async Task<IActionResult> UpdateEmployeeAdjustment([FromBody] UpdateDetailAdjustmentRequest request)
        {
            if (request == null || request.DetailId <= 0) return BadRequest("Valid DetailId required");
            try
            {
                var success = await _payrollService.UpdateEmployeeAdjustmentAsync(
                    request.DetailId,
                    request.ManualAdjustmentAmount,
                    request.ArrearsAmount,
                    request.BonusAmount,
                    request.AdjustmentReason);

                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Line-item adjustments updated successfully",
                    Data = success
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("individual-credit")]
        [RequirePermission("PAYROLL_PROCESS")]
        public async Task<IActionResult> RecordIndividualSalaryCredit([FromBody] IndividualSalaryCreditRequest request)
        {
            if (request == null || request.EmployeeId <= 0)
            {
                return BadRequest(new ApiResponse<object> { StatusCode = 400, Message = "Valid EmployeeId required" });
            }

            try
            {
                var payslip = await _payrollService.RecordIndividualSalaryCreditAsync(request);
                return Ok(new ApiResponse<PayslipDto>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Individual salary credit recorded and payslip generated successfully",
                    Data = payslip
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message
                });
            }
        }

        #endregion

        #region 3. Bank Payout File Generation

        [HttpGet("payrun/{id}/export-bank-payout")]
        [RequirePermission("PAYROLL_PROCESS")]
        public async Task<IActionResult> ExportBankPayout(long id)
        {
            var bytes = await _payrollService.GenerateBankPayoutCsvAsync(id);
            string filename = $"BankPayout_PayRun_{id}_{DateTime.UtcNow:yyyyMMdd}.csv";
            return File(bytes, "text/csv", filename);
        }

        #endregion

        #region 4. Payslips & Employee Self-Service

        [HttpGet("payslips/{employeeId?}")]
        [RequirePermission("PAYROLL_VIEW_SELF")]
        public async Task<IActionResult> GetPayslips(long? employeeId)
        {
            long currentEmpId = _userContext.GetCurrentEmployeeId();
            bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR() || User.IsInRole("Admin") || User.IsInRole("HR") || await _userContext.HasPermissionAsync("PAYROLL_VIEW_ALL");
            if (isPrivileged && (!employeeId.HasValue || employeeId.Value <= 0))
            {
                var allPayslips = await _payrollService.GetAllPayslipsAsync();
                return Ok(new ApiResponse<IEnumerable<PayslipDto>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Payslips retrieved",
                    Data = allPayslips
                });
            }

            long targetId = employeeId.HasValue && employeeId.Value > 0 ? employeeId.Value : currentEmpId;

            if (!isPrivileged && targetId != currentEmpId)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new ApiResponse<IEnumerable<PayslipDto>>
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = "Access Denied: You do not have permission to view another employee's payslips.",
                    Data = null
                });
            }

            var payslips = await _payrollService.GetEmployeePayslipsAsync(targetId);
            return Ok(new ApiResponse<IEnumerable<PayslipDto>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Payslips retrieved",
                Data = payslips
            });
        }

        [HttpGet("payslip/{id}/view")]
        [RequirePermission("PAYROLL_VIEW_SELF")]
        public async Task<IActionResult> ViewPayslipHtml(long id)
        {
            var dto = await _payrollService.GetPayslipByIdAsync(id);
            if (dto == null) return NotFound(new ApiResponse<object> { StatusCode = 404, Message = "Payslip not found" });

            long currentEmpId = _userContext.GetCurrentEmployeeId();
            bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR() || User.IsInRole("Admin") || User.IsInRole("HR") || await _userContext.HasPermissionAsync("PAYROLL_VIEW_ALL");
            if (!isPrivileged && dto.EmployeeId != currentEmpId)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Access Denied: You do not have permission to view this payslip.");
            }

            var html = await _payrollService.GeneratePayslipHtmlAsync(id);
            return Content(html, "text/html");
        }

        [HttpGet("payslip/{id}")]
        [RequirePermission("PAYROLL_VIEW_SELF")]
        public async Task<IActionResult> GetPayslip(long id)
        {
            var dto = await _payrollService.GetPayslipByIdAsync(id);
            if (dto == null) return NotFound(new ApiResponse<object> { StatusCode = 404, Message = "Payslip not found" });

            long currentEmpId = _userContext.GetCurrentEmployeeId();
            bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR() || User.IsInRole("Admin") || User.IsInRole("HR") || await _userContext.HasPermissionAsync("PAYROLL_VIEW_ALL");
            if (!isPrivileged && dto.EmployeeId != currentEmpId)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new ApiResponse<PayslipDto>
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = "Access Denied: You do not have permission to view this payslip.",
                    Data = null
                });
            }

            return Ok(new ApiResponse<PayslipDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Payslip retrieved",
                Data = dto
            });
        }

        #endregion

        #region 5. Reimbursements & FBP

        [HttpGet("reimbursements/{employeeId?}")]
        [RequirePermission("PAYROLL_VIEW_SELF")]
        public async Task<IActionResult> GetReimbursements(long? employeeId)
        {
            long currentEmpId = _userContext.GetCurrentEmployeeId();
            long targetId = employeeId.HasValue && employeeId.Value > 0 ? employeeId.Value : currentEmpId;

            bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR() || User.IsInRole("Admin") || User.IsInRole("HR") || await _userContext.HasPermissionAsync("PAYROLL_VIEW_ALL");

            if (!isPrivileged && targetId != currentEmpId)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new ApiResponse<IEnumerable<ReimbursementClaim>>
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = "Access Denied: You do not have permission to view another employee's reimbursements.",
                    Data = null
                });
            }

            var claims = await _payrollService.GetEmployeeReimbursementsAsync(targetId);
            return Ok(new ApiResponse<IEnumerable<ReimbursementClaim>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Reimbursements retrieved",
                Data = claims
            });
        }

        [HttpGet("reimbursements-pending")]
        [RequirePermission("PAYROLL_VIEW_ALL")]
        public async Task<IActionResult> GetAllPendingReimbursements()
        {
            var claims = await _payrollService.GetAllPendingReimbursementsAsync();
            return Ok(new ApiResponse<IEnumerable<ReimbursementClaim>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "All reimbursements retrieved",
                Data = claims
            });
        }

        [HttpPost("reimbursements/submit")]
        [RequirePermission("PAYROLL_VIEW_SELF")]
        public async Task<IActionResult> SubmitReimbursement([FromBody] ReimbursementClaimRequest request)
        {
            if (request == null || request.Amount <= 0) return BadRequest("Valid claim amount required");

            long currentEmpId = _userContext.GetCurrentEmployeeId();
            bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR();
            if (!isPrivileged && currentEmpId > 0)
            {
                request.EmployeeId = currentEmpId;
            }

            var result = await _payrollService.SubmitReimbursementClaimAsync(request);
            return Ok(new ApiResponse<ReimbursementClaim>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Reimbursement claim submitted successfully",
                Data = result
            });
        }

        [HttpPost("reimbursements/review")]
        [RequirePermission("PAYROLL_PROCESS")]
        public async Task<IActionResult> ReviewReimbursement([FromBody] ReviewReimbursementRequest request)
        {
            if (request == null || request.ClaimId <= 0) return BadRequest("Valid ClaimId required");
            var result = await _payrollService.ReviewReimbursementClaimAsync(request);
            return Ok(new ApiResponse<ReimbursementClaim>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = $"Reimbursement claim marked as {request.Status}",
                Data = result
            });
        }

        #endregion

        #region 6. Full & Final Settlement (FnF)

        [HttpPost("fnf/calculate")]
        [RequirePermission("PAYROLL_PROCESS")]
        public async Task<IActionResult> CalculateFnF([FromBody] FullAndFinalCalculationRequest request)
        {
            if (request == null || request.EmployeeId <= 0) return BadRequest("Valid EmployeeId required");
            var result = await _payrollService.CalculateFnFSettlementAsync(request);
            return Ok(new ApiResponse<FullAndFinalSettlement>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "FnF settlement calculated successfully",
                Data = result
            });
        }

        [HttpGet("fnf/{employeeId}")]
        public async Task<IActionResult> GetFnF(long employeeId)
        {
            long currentEmpId = _userContext.GetCurrentEmployeeId();
            bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR() || User.IsInRole("Admin") || User.IsInRole("HR") || await _userContext.HasPermissionAsync("PAYROLL_VIEW_ALL");

            if (!isPrivileged && employeeId != currentEmpId)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new ApiResponse<FullAndFinalSettlement?>
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = "Access Denied: You do not have permission to view another employee's FnF settlement.",
                    Data = null
                });
            }

            var fnf = await _payrollService.GetFnFSettlementByEmployeeIdAsync(employeeId);
            return Ok(new ApiResponse<FullAndFinalSettlement?>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = fnf != null ? "FnF found" : "No FnF record found",
                Data = fnf
            });
        }

        [HttpPost("fnf/{id}/status")]
        [RequirePermission("PAYROLL_PROCESS")]
        public async Task<IActionResult> UpdateFnFStatus(long id, [FromQuery] string status, [FromQuery] string? remarks)
        {
            var result = await _payrollService.UpdateFnFStatusAsync(id, status, remarks);
            return Ok(new ApiResponse<FullAndFinalSettlement>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = $"FnF status updated to {status}",
                Data = result
            });
        }

        #endregion

        #region 7. Employee Loans & Advances

        [HttpGet("loans/{employeeId?}")]
        public async Task<IActionResult> GetLoans(long? employeeId)
        {
            long currentEmpId = _userContext.GetCurrentEmployeeId();
            bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR() || User.IsInRole("Admin") || User.IsInRole("HR") || await _userContext.HasPermissionAsync("PAYROLL_VIEW_ALL");

            if (!isPrivileged)
            {
                employeeId = currentEmpId;
            }

            var loans = await _payrollService.GetAllLoansAsync(employeeId);
            return Ok(new ApiResponse<IEnumerable<EmployeeLoan>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Loans and advances retrieved successfully",
                Data = loans
            });
        }

        [HttpPost("loans/apply")]
        public async Task<IActionResult> ApplyLoan([FromBody] ApplyEmployeeLoanRequest request)
        {
            if (request == null) return BadRequest("Invalid loan application data");
            long currentEmpId = _userContext.GetCurrentEmployeeId();
            bool isPrivileged = _userContext.IsAdmin() || _userContext.IsHR() || User.IsInRole("Admin") || User.IsInRole("HR");

            if (!isPrivileged || request.EmployeeId <= 0)
            {
                request.EmployeeId = currentEmpId;
            }

            try
            {
                var result = await _payrollService.ApplyLoanAsync(request, currentEmpId);
                return Ok(new ApiResponse<EmployeeLoan>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Loan/Advance application submitted successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("loans/status")]
        [RequirePermission("PAYROLL_PROCESS")]
        public async Task<IActionResult> UpdateLoanStatus([FromBody] UpdateEmployeeLoanStatusRequest request)
        {
            if (request == null || request.LoanId <= 0) return BadRequest("Valid LoanId required");
            try
            {
                var result = await _payrollService.UpdateLoanStatusAsync(request.LoanId, request.Status, request.Reason);
                return Ok(new ApiResponse<EmployeeLoan>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = $"Loan status updated to {request.Status}",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message
                });
            }
        }

        #endregion
    }
}
