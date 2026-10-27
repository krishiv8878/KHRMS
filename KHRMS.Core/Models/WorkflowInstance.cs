using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("WorkflowInstances")]
    public class WorkflowInstance : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string RequestType { get; set; } = string.Empty; // "Leave", "AttendanceRegularization", "Timesheet", "Expense"

        [Required]
        public long RequestId { get; set; } // Reference to LeaveRequest.Id, AttendanceRequest.Id, etc.

        [Required]
        public long RequesterEmployeeId { get; set; }

        [ForeignKey("RequesterEmployeeId")]
        public virtual Employee? RequesterEmployee { get; set; }

        [Required]
        public long WorkflowDefinitionId { get; set; }

        [ForeignKey("WorkflowDefinitionId")]
        public virtual WorkflowDefinition? WorkflowDefinition { get; set; }

        public int DefinitionVersion { get; set; } = 1;

        public int CurrentStepOrder { get; set; } = 1;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // "Pending", "Approved", "Rejected", "Cancelled"

        public DateTime StartedDate { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedDate { get; set; }

        public long? FinalActionBy { get; set; }

        [StringLength(1000)]
        public string? FinalComment { get; set; }

        public virtual ICollection<WorkflowInstanceStep> InstanceSteps { get; set; } = new List<WorkflowInstanceStep>();
        public virtual ICollection<WorkflowAuditLog> AuditLogs { get; set; } = new List<WorkflowAuditLog>();
    }
}
