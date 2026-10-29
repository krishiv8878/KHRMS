using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("WorkflowDefinitions")]
    public class WorkflowDefinition : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string RequestType { get; set; } = string.Empty; // e.g. "Leave", "AttendanceRegularization", "Timesheet", "Expense"

        public int Version { get; set; } = 1;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public bool IsPublished { get; set; } = false;

        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;

        public bool IsDefault { get; set; } = false;

        public virtual ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
        public virtual ICollection<WorkflowRoutingRule> RoutingRules { get; set; } = new List<WorkflowRoutingRule>();
        public virtual ICollection<WorkflowInstance> Instances { get; set; } = new List<WorkflowInstance>();
    }
}
