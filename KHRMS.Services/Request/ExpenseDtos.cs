using System.ComponentModel.DataAnnotations;
using KHRMS.Core.Models;

namespace KHRMS.Services.Request
{
    public class ExpenseMileageDto
    {
        public string VehicleType { get; set; } = "Two-Wheeler";
        public string StartLocation { get; set; } = string.Empty;
        public string EndLocation { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public decimal RatePerKm { get; set; }
        public decimal CalculatedAmount { get; set; }
    }

    public class CreateExpenseItemRequest
    {
        public long Id { get; set; }
        public long ExpenseCategoryId { get; set; }
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
        public string? MerchantName { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public string? Description { get; set; }
        public string? ReceiptUrl { get; set; }
        public ExpenseMileageDto? Mileage { get; set; }
    }

    public class CreateExpenseReportRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Purpose { get; set; }
        public long? ProjectId { get; set; }
        public long? AdvanceIdToAdjust { get; set; }
        public List<CreateExpenseItemRequest> Items { get; set; } = new();
    }

    public class UpdateExpenseReportRequest
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Purpose { get; set; }
        public long? ProjectId { get; set; }
        public long? AdvanceIdToAdjust { get; set; }
        public List<CreateExpenseItemRequest> Items { get; set; } = new();
    }

    public class ExpenseItemDecisionDto
    {
        public long ItemId { get; set; }
        public string Status { get; set; } = "Approved"; // Approved, Rejected
        public decimal ApprovedAmount { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class ReviewExpenseReportRequest
    {
        public long ReportId { get; set; }
        public string Action { get; set; } = "ManagerApprove"; // ManagerApprove, FinanceApprove, Reject
        public decimal ApprovedAmount { get; set; }
        public string? Remarks { get; set; }
        public List<ExpenseItemDecisionDto>? ItemDecisions { get; set; }
    }

    public class DisburseExpenseReportRequest
    {
        public long ReportId { get; set; }
        public string PaymentMode { get; set; } = "DirectBank"; // DirectBank, Payroll, Cash
        public long? PayRunId { get; set; }
    }

    public class CreateExpenseAdvanceRequest
    {
        public decimal AmountRequested { get; set; }
        [Required]
        public string Purpose { get; set; } = string.Empty;
        public DateTime? TravelStartDate { get; set; }
        public DateTime? TravelEndDate { get; set; }
    }

    public class ReviewExpenseAdvanceRequest
    {
        public long AdvanceId { get; set; }
        public string Action { get; set; } = "Approve"; // Approve, Reject, Disburse
        public decimal ApprovedAmount { get; set; }
        public string? Remarks { get; set; }
    }

    public class ExpenseItemDetailDto
    {
        public ExpenseItem Item { get; set; } = null!;
        public string CategoryName { get; set; } = string.Empty;
        public ExpenseMileage? Mileage { get; set; }
    }

    public class ExpenseReportDetailDto
    {
        public ExpenseReport Report { get; set; } = null!;
        public List<ExpenseItemDetailDto> Items { get; set; } = new();
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string DesignationName { get; set; } = string.Empty;
        public string? ProjectName { get; set; }
        public string? ManagerName { get; set; }
    }

    public class ExpenseAdvanceDto
    {
        public ExpenseAdvance Advance { get; set; } = null!;
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string DesignationName { get; set; } = string.Empty;
    }
}
