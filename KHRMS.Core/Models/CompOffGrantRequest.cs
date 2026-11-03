using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    public class CompOffGrantRequest : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey("Employee")]
        public long EmployeeId { get; set; }
        public virtual Employee? Employee { get; set; }

        [Required]
        public DateTime AttendanceDate { get; set; }

        public int WorkedMinutes { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal EligibleDays { get; set; } = 1.0m; // 0.5 or 1.0

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
