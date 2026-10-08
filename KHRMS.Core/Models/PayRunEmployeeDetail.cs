using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("PayRunEmployeeDetails")]
    public class PayRunEmployeeDetail : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey("PayRun")]
        public long PayRunId { get; set; }

        [Required]
        [ForeignKey("Employee")]
        public long EmployeeId { get; set; }

        [StringLength(100)]
        public string? EmployeeName { get; set; }

        [StringLength(50)]
        public string? EmployeeCode { get; set; }

        public int TotalWorkingDays { get; set; }

        public decimal PresentDays { get; set; }

        public decimal PaidLeaveDays { get; set; }

        public decimal LopDays { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LopDeduction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasicPay { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Hra { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SpecialAllowance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Reimbursements { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GrossPay { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EpfDeduction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EsicDeduction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PtDeduction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TdsDeduction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OtherDeductions { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDeductions { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetPay { get; set; }

        [StringLength(30)]
        public string PaymentStatus { get; set; } = "Pending"; // "Pending", "Processed", "Failed"

        [StringLength(100)]
        public string? BankName { get; set; }

        public long? AccountNumber { get; set; }

        [StringLength(20)]
        public string? IfscCode { get; set; }
    }
}
