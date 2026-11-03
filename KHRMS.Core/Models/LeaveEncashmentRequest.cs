using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    public class LeaveEncashmentRequest : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey("Employee")]
        public long EmployeeId { get; set; }
        public virtual Employee? Employee { get; set; }

        [Required]
        [ForeignKey("LeaveType")]
        public long LeaveTypeId { get; set; }
        public virtual LeaveType? LeaveType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal RequestedDays { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DailySalaryBasis { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedAmount { get; set; } = 0;

        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, ProcessedInPayroll

        public long? ActionBy { get; set; }
        public DateTime? ActionDate { get; set; }

        [StringLength(500)]
        public string? RejectionReason { get; set; }

        public long? PayRunId { get; set; } // Linked when processed in payroll

        public long? WorkflowInstanceId { get; set; }
    }
}
