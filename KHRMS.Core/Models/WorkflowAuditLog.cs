using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("WorkflowAuditLogs")]
    public class WorkflowAuditLog : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long WorkflowInstanceId { get; set; }

        [ForeignKey("WorkflowInstanceId")]
        public virtual WorkflowInstance? WorkflowInstance { get; set; }

        public long? WorkflowInstanceStepId { get; set; }

        [ForeignKey("WorkflowInstanceStepId")]
        public virtual WorkflowInstanceStep? WorkflowInstanceStep { get; set; }

        [Required]
        [StringLength(100)]
        public string EventType { get; set; } = string.Empty; // "WorkflowStarted", "StepAssigned", "StepApproved", "StepRejected", "StepOverridden", "StepReassigned", "StepEscalated", "WorkflowCancelled"

        public long? PerformedByEmployeeId { get; set; }

        [ForeignKey("PerformedByEmployeeId")]
        public virtual Employee? PerformedByEmployee { get; set; }

        [StringLength(50)]
        public string? ActionCapacity { get; set; } // "Approver", "Delegate", "Override", "System"

        [Required]
        [StringLength(2000)]
        public string Details { get; set; } = string.Empty;

        [StringLength(50)]
        public string? OldStatus { get; set; }

        [StringLength(50)]
        public string? NewStatus { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
