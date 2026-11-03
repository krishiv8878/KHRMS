using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("EmployeeLoans")]
    public class EmployeeLoan : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey("Employee")]
        public long EmployeeId { get; set; }

        public virtual Employee? Employee { get; set; }

        [Required]
        [StringLength(50)]
        public string LoanType { get; set; } = "Personal Loan"; // "Personal Loan", "Salary Advance", "Emergency Loan"

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrincipalAmount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal InterestRatePercent { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalRepayableAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyEmiAmount { get; set; }

        public int TenureMonths { get; set; }

        public DateTime DisbursementDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingBalance { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Active"; // "Active", "Closed", "Defaulted"

        public long? WorkflowInstanceId { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }
    }
}
