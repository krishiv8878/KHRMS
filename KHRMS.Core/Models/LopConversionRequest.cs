using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    public class LopConversionRequest : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey("Employee")]
        public long EmployeeId { get; set; }
        public virtual Employee? Employee { get; set; }

        [Required]
        [ForeignKey("OriginalLeaveRequest")]
        public long OriginalLeaveRequestId { get; set; }
        public virtual LeaveRequest? OriginalLeaveRequest { get; set; }

        [Required]
        [ForeignKey("TargetLeaveType")]
        public long TargetLeaveTypeId { get; set; }
        public virtual LeaveType? TargetLeaveType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ConversionDays { get; set; }

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        public long? ActionBy { get; set; }
        public DateTime? ActionDate { get; set; }

        [StringLength(500)]
        public string? RejectionReason { get; set; }

        public long? WorkflowInstanceId { get; set; }
    }
}
