using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("ExpenseCategories")]
    public class ExpenseCategory : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MaxLimitPerClaim { get; set; } = 50000;

        public bool RequiresReceipt { get; set; } = true;

        [Column(TypeName = "decimal(18,2)")]
        public decimal ReceiptThresholdAmount { get; set; } = 500;

        public bool IsMileageCategory { get; set; } = false;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MileageRatePerKm { get; set; } = 0;
    }

    [Table("ExpenseReports")]
    public class ExpenseReport : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey("Employee")]
        public long EmployeeId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Purpose { get; set; }

        public long? ProjectId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalClaimedAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalApprovedAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AdvanceAdjustedAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetPayableAmount { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Draft"; // Draft, Submitted, ManagerApproved, FinanceApproved, Paid, Rejected

        public DateTime? SubmittedDate { get; set; }

        public long? ManagerId { get; set; }

        public DateTime? ManagerActionDate { get; set; }

        [StringLength(500)]
        public string? ManagerRemarks { get; set; }

        public long? FinanceApprovedBy { get; set; }

        public DateTime? FinanceActionDate { get; set; }

        [StringLength(500)]
        public string? FinanceRemarks { get; set; }

        public DateTime? PaidDate { get; set; }

        [StringLength(50)]
        public string? PaymentMode { get; set; } // Payroll, DirectBank, Cash

        public long? PayRunId { get; set; }
    }

    [Table("ExpenseItems")]
    public class ExpenseItem : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey("ExpenseReport")]
        public long ExpenseReportId { get; set; }

        [Required]
        [ForeignKey("ExpenseCategory")]
        public long ExpenseCategoryId { get; set; }

        public DateTime ExpenseDate { get; set; }

        [StringLength(150)]
        public string? MerchantName { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(300)]
        public string? ReceiptUrl { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ApprovedAmount { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        [StringLength(300)]
        public string? RejectionReason { get; set; }
    }

    [Table("ExpenseMileages")]
    public class ExpenseMileage : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey("ExpenseItem")]
        public long ExpenseItemId { get; set; }

        [Required]
        [StringLength(50)]
        public string VehicleType { get; set; } = "Two-Wheeler"; // Two-Wheeler, Four-Wheeler

        [StringLength(200)]
        public string StartLocation { get; set; } = string.Empty;

        [StringLength(200)]
        public string EndLocation { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DistanceKm { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RatePerKm { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CalculatedAmount { get; set; }
    }

    [Table("ExpenseAdvances")]
    public class ExpenseAdvance : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey("Employee")]
        public long EmployeeId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountRequested { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ApprovedAmount { get; set; }

        [Required]
        [StringLength(300)]
        public string Purpose { get; set; } = string.Empty;

        public DateTime? TravelStartDate { get; set; }

        public DateTime? TravelEndDate { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Requested"; // Requested, Approved, Disbursed, Settled, Rejected

        public long? ApprovedBy { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public DateTime? DisbursedDate { get; set; }

        public long? SettledReportId { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}
