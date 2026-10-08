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
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseService _expenseService;
        private readonly IUserContextService _userContext;

        public ExpenseController(IExpenseService expenseService, IUserContextService userContext)
        {
            _expenseService = expenseService;
            _userContext = userContext;
        }

        #region 1. Categories

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories([FromQuery] bool includeInactive = false)
        {
            var list = await _expenseService.GetCategoriesAsync(includeInactive);
            return Ok(new ApiResponse<List<ExpenseCategory>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Expense categories retrieved successfully",
                Data = list
            });
        }

        [HttpPost("categories")]
        [RequirePermission("EXPENSE_MANAGE")]
        public async Task<IActionResult> SaveCategory([FromBody] ExpenseCategory category)
        {
            var result = await _expenseService.SaveCategoryAsync(category);
            return Ok(new ApiResponse<ExpenseCategory>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Expense category saved successfully",
                Data = result
            });
        }

        [HttpDelete("categories/{id}")]
        [RequirePermission("EXPENSE_MANAGE")]
        public async Task<IActionResult> DeleteCategory(long id)
        {
            var success = await _expenseService.DeleteCategoryAsync(id);
            if (!success) return NotFound(new ApiResponse<string> { StatusCode = (int)HttpStatusCode.NotFound, Message = "Category not found" });

            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Category removed successfully",
                Data = true
            });
        }

        #endregion

        #region 2. Expense Reports & Claims

        [HttpGet("reports")]
        public async Task<IActionResult> GetReports([FromQuery] long? employeeId, [FromQuery] string? status, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            // If regular employee, only show their own reports
            var isApprover = User.IsInRole("Admin") || User.IsInRole("System Admin") || User.IsInRole("HR") || User.IsInRole("HR Operations") || User.IsInRole("Manager") || User.IsInRole("Management");
            long currentEmpId = _userContext.GetCurrentEmployeeId();
            var filterEmpId = isApprover ? employeeId : (currentEmpId > 0 ? currentEmpId : employeeId);

            var reports = await _expenseService.GetReportsAsync(filterEmpId, status, fromDate, toDate);
            return Ok(new ApiResponse<List<ExpenseReportDetailDto>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Expense reports retrieved successfully",
                Data = reports
            });
        }

        [HttpGet("reports/{id}")]
        public async Task<IActionResult> GetReportById(long id)
        {
            var report = await _expenseService.GetReportByIdAsync(id);
            if (report == null) return NotFound(new ApiResponse<string> { StatusCode = (int)HttpStatusCode.NotFound, Message = "Expense report not found" });

            return Ok(new ApiResponse<ExpenseReportDetailDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Expense report details retrieved successfully",
                Data = report
            });
        }

        [HttpPost("reports")]
        public async Task<IActionResult> CreateReport([FromBody] CreateExpenseReportRequest request)
        {
            var result = await _expenseService.CreateReportAsync(request);
            return Ok(new ApiResponse<ExpenseReportDetailDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Expense report created successfully",
                Data = result
            });
        }

        [HttpPut("reports/{id}")]
        public async Task<IActionResult> UpdateReport(long id, [FromBody] UpdateExpenseReportRequest request)
        {
            request.Id = id;
            var result = await _expenseService.UpdateReportAsync(request);
            return Ok(new ApiResponse<ExpenseReportDetailDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Expense report updated successfully",
                Data = result
            });
        }

        [HttpDelete("reports/{id}")]
        public async Task<IActionResult> DeleteReport(long id)
        {
            var success = await _expenseService.DeleteReportAsync(id);
            if (!success) return NotFound(new ApiResponse<string> { StatusCode = (int)HttpStatusCode.NotFound, Message = "Expense report not found" });

            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Expense report deleted successfully",
                Data = true
            });
        }

        [HttpPost("reports/{id}/submit")]
        public async Task<IActionResult> SubmitReport(long id)
        {
            var result = await _expenseService.SubmitReportAsync(id);
            return Ok(new ApiResponse<ExpenseReportDetailDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Expense report submitted for approval",
                Data = result
            });
        }

        #endregion

        #region 3. Approvals & Disbursement

        [HttpPost("reports/review")]
        [RequirePermission("EXPENSE_APPROVE")]
        public async Task<IActionResult> ReviewReport([FromBody] ReviewExpenseReportRequest request)
        {
            var result = await _expenseService.ReviewReportAsync(request);
            return Ok(new ApiResponse<ExpenseReportDetailDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Expense report review action recorded successfully",
                Data = result
            });
        }

        [HttpPost("reports/disburse")]
        [RequirePermission("EXPENSE_MANAGE")]
        public async Task<IActionResult> DisburseReport([FromBody] DisburseExpenseReportRequest request)
        {
            var result = await _expenseService.DisburseReportAsync(request);
            return Ok(new ApiResponse<ExpenseReportDetailDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Expense report reimbursement marked as disbursed",
                Data = result
            });
        }

        #endregion

        #region 4. Cash Advances

        [HttpGet("advances")]
        public async Task<IActionResult> GetAdvances([FromQuery] long? employeeId, [FromQuery] string? status)
        {
            var isApprover = User.IsInRole("Admin") || User.IsInRole("System Admin") || User.IsInRole("HR") || User.IsInRole("HR Operations") || User.IsInRole("Manager") || User.IsInRole("Management");
            long currentEmpId = _userContext.GetCurrentEmployeeId();
            var filterEmpId = isApprover ? employeeId : (currentEmpId > 0 ? currentEmpId : employeeId);

            var advances = await _expenseService.GetAdvancesAsync(filterEmpId, status);
            return Ok(new ApiResponse<List<ExpenseAdvanceDto>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Expense advances retrieved successfully",
                Data = advances
            });
        }

        [HttpPost("advances")]
        public async Task<IActionResult> RequestAdvance([FromBody] CreateExpenseAdvanceRequest request)
        {
            var result = await _expenseService.RequestAdvanceAsync(request);
            return Ok(new ApiResponse<ExpenseAdvanceDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Cash advance requested successfully",
                Data = result
            });
        }

        [HttpPost("advances/review")]
        [RequirePermission("EXPENSE_APPROVE")]
        public async Task<IActionResult> ReviewAdvance([FromBody] ReviewExpenseAdvanceRequest request)
        {
            var result = await _expenseService.ReviewAdvanceAsync(request);
            return Ok(new ApiResponse<ExpenseAdvanceDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Advance review status updated successfully",
                Data = result
            });
        }

        #endregion
    }
}
