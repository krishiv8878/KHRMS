using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("WorkflowRoutingRules")]
    public class WorkflowRoutingRule : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string RequestType { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ScopeType { get; set; } = "Default"; // "Default", "Department", "Location", "Project"

        [StringLength(150)]
        public string? ScopeValue { get; set; } // Department name, Branch name, or Project Id string

        [Required]
        public long WorkflowDefinitionId { get; set; }

        [ForeignKey("WorkflowDefinitionId")]
        public virtual WorkflowDefinition? WorkflowDefinition { get; set; }

        [StringLength(100)]
        public string? ApproverRule { get; set; }

        public int Priority { get; set; } = 100; // Lowest number wins (e.g. 10 overrides 100; Default is 999)

        public bool IsDefault { get; set; } = false;
    }
}
