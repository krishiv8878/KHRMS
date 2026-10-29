using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("WorkflowSteps")]
    public class WorkflowStep : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long WorkflowDefinitionId { get; set; }

        [ForeignKey("WorkflowDefinitionId")]
        public virtual WorkflowDefinition? WorkflowDefinition { get; set; }

        public int StepOrder { get; set; } = 1;

        [Required]
        [StringLength(150)]
        public string StepName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ApproverRuleType { get; set; } = string.Empty; // "ReportingManager", "SkipLevelManager", "DepartmentHead", "RoleInLocation", "ProjectOwner", "SpecificPerson", "GroupQueue"

        public string? RuleParameters { get; set; } // JSON parameters for rule e.g. {"RoleId": 2}, {"EmployeeId": 105}

        [Required]
        [StringLength(30)]
        public string StepMode { get; set; } = "AnyOne"; // "AnyOne" or "All"

        public string? ConditionJson { get; set; } // Optional condition e.g. {"field":"Days","operator":"GreaterThan","value":5}

        public int? TimeLimitHours { get; set; }

        /// <summary>
        /// Admin toggle to turn auto-escalation on or off for this step.
        /// </summary>
        public bool IsEscalationEnabled { get; set; } = false;

        [StringLength(100)]
        public string? EscalationTarget { get; set; } // e.g. "SkipLevelManager", "DepartmentHead", "Role:HR", "SpecificPerson:102"

        [StringLength(50)]
        public string? EscalationAction { get; set; } = "Reassign"; // "Reassign", "Notify"
    }
}
