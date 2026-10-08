using KHRMS.Core.Models;
using KHRMS.Services.Request;

namespace KHRMS.Services.Interfaces
{
    public interface IExpenseService
    {
        // 1. Categories & Policy
        Task<List<ExpenseCategory>> GetCategoriesAsync(bool includeInactive = false);
        Task<ExpenseCategory> SaveCategoryAsync(ExpenseCategory category);
        Task<bool> DeleteCategoryAsync(long id);

        // 2. Expense Reports & Claims
        Task<List<ExpenseReportDetailDto>> GetReportsAsync(long? employeeId, string? status, DateTime? fromDate, DateTime? toDate);
        Task<ExpenseReportDetailDto?> GetReportByIdAsync(long id);
        Task<ExpenseReportDetailDto> CreateReportAsync(CreateExpenseReportRequest request);
        Task<ExpenseReportDetailDto> UpdateReportAsync(UpdateExpenseReportRequest request);
        Task<bool> DeleteReportAsync(long id);
        Task<ExpenseReportDetailDto> SubmitReportAsync(long id);

        // 3. Approvals & Audit
        Task<ExpenseReportDetailDto> ReviewReportAsync(ReviewExpenseReportRequest request);
        Task<ExpenseReportDetailDto> DisburseReportAsync(DisburseExpenseReportRequest request);

        // 4. Cash Advances
        Task<List<ExpenseAdvanceDto>> GetAdvancesAsync(long? employeeId, string? status);
        Task<ExpenseAdvanceDto> RequestAdvanceAsync(CreateExpenseAdvanceRequest request);
        Task<ExpenseAdvanceDto> ReviewAdvanceAsync(ReviewExpenseAdvanceRequest request);
    }
}
