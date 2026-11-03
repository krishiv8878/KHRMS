using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    public class LeaveEntitlement : KHRMSBase
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
        [StringLength(50)]
        public string SourceType { get; set; } = "Accrual"; // Opening, Accrual, CarryForward, CompOff

        public long? SourceReferenceId { get; set; } // Reference to Attendance record or Ledger entry

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDays { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ConsumedDays { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingDays { get; set; }

        public DateTime CreditDate { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiryDate { get; set; }

        public bool IsExpired { get; set; } = false;
    }
}
