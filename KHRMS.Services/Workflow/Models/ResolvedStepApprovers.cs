using System.Collections.Generic;

namespace KHRMS.Services.Workflow.Models
{
    public class ResolvedStepApprovers
    {
        public int StepOrder { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string StepMode { get; set; } = "AnyOne"; // "AnyOne" or "All"
        public string ApproverRuleType { get; set; } = string.Empty;
        public List<ResolvedApproverItem> Approvers { get; set; } = new();
        public string ResolutionReason { get; set; } = string.Empty;
        public int? TimeLimitHours { get; set; }
        public bool IsEscalationEnabled { get; set; }
        public string? EscalationTarget { get; set; }
        public string? EscalationAction { get; set; } = "Reassign";
        public bool ConditionSatisfied { get; set; } = true;
    }
}
