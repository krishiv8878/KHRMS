using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("WorkflowInstanceSteps")]
    public class WorkflowInstanceStep : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long WorkflowInstanceId { get; set; }

        [ForeignKey("WorkflowInstanceId")]
        public virtual WorkflowInstance? WorkflowInstance { get; set; }

        public int StepOrder { get; set; } = 1;

        [Required]
        [StringLength(150)]
        public string StepName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // "Pending", "Approved", "Rejected", "Skipped", "Escalated"

        [Required]
        [StringLength(100)]
        public string ApproverRuleType { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ResolutionReason { get; set; }

        /// <summary>
        /// JSON array of eligible employee IDs snapshotted when this step starts (e.g. "[102, 105]").
        /// </summary>
        [Required]
        public string EligibleApproverIdsJson { get; set; } = "[]";

        [Required]
        [StringLength(30)]
        public string StepMode { get; set; } = "AnyOne"; // "AnyOne" or "All"

        public long? ActionByEmployeeId { get; set; }

        [ForeignKey("ActionByEmployeeId")]
        public virtual Employee? ActionByEmployee { get; set; }

        [StringLength(50)]
        public string? ActionCapacity { get; set; } // "Approver", "Delegate", "Override"

        [StringLength(50)]
        public string? ActionDecision { get; set; } // "Approved", "Rejected"

        [StringLength(1000)]
        public string? Comment { get; set; }

        public DateTime StepStartTime { get; set; } = DateTime.UtcNow;

        public DateTime? ActionTime { get; set; }

        public DateTime? DueTime { get; set; }

        public bool IsEscalated { get; set; } = false;

        public DateTime? EscalatedTime { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        public virtual ICollection<WorkflowAuditLog> AuditLogs { get; set; } = new List<WorkflowAuditLog>();
    }
}
