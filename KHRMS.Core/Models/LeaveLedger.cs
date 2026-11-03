using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    public class LeaveLedger : KHRMSBase
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
        public string TransactionType { get; set; } = string.Empty; 
        // OpeningBalance, Accrual, CarryForward, Reservation, Deduction, Restoration, 
        // CompOffCredit, CompOffExpiry, Lapse, Encashment, LOPToPaidConversion, Adjustment

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Days { get; set; } // Positive for credits/restorations, negative for deductions/encashment/lapse

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAfter { get; set; }

        [ForeignKey("LeaveRequest")]
        public long? LeaveRequestId { get; set; }
        public virtual LeaveRequest? LeaveRequest { get; set; }

        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string? Period { get; set; } // e.g., "2026-11", "2026-YearEnd"

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}
