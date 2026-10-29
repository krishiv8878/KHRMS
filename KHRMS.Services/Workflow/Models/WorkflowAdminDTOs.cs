using System;
using System.Collections.Generic;

namespace KHRMS.Services.Workflow.Models
{
    public class WorkflowDefinitionSummaryDTO
    {
        public long Id { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public int Version { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPublished { get; set; }
        public DateTime EffectiveDate { get; set; }
        public int StepCount { get; set; }
        public int RoutingRuleCount { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string? LastModifiedBy { get; set; }
    }

    public class WorkflowStepConfigDTO
    {
        public long? Id { get; set; }
        public int StepOrder { get; set; } = 1;
        public string StepName { get; set; } = string.Empty;
        public string ApproverRuleType { get; set; } = "ReportingManager";
        public string? RuleParameters { get; set; }
        public string StepMode { get; set; } = "AnyOne"; // "AnyOne" or "All"
        public string? ConditionJson { get; set; }
        public int? TimeLimitHours { get; set; }
        public bool IsEscalationEnabled { get; set; }
        public string? EscalationTarget { get; set; }
        public string? EscalationAction { get; set; } = "Reassign";
    }

    public class WorkflowRoutingRuleConfigDTO
    {
        public long? Id { get; set; }
        public string ScopeType { get; set; } = "Default"; // "Default", "Department", "Location", "Project"
        public string? ScopeValue { get; set; }
        public string? ApproverRule { get; set; }
        public int Priority { get; set; } = 100;
        public bool IsDefault { get; set; } = false;
    }

    public class WorkflowDefinitionDetailDTO
    {
        public WorkflowDefinitionSummaryDTO Definition { get; set; } = new();
        public List<WorkflowStepConfigDTO> Steps { get; set; } = new();
        public List<WorkflowRoutingRuleConfigDTO> RoutingRules { get; set; } = new();
    }

    public class WorkflowDefinitionSaveDTO
    {
        public string RequestType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public List<WorkflowStepConfigDTO> Steps { get; set; } = new();
        public List<WorkflowRoutingRuleConfigDTO> RoutingRules { get; set; } = new();
    }

    public class WorkflowValidationResultDTO
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    public class TestRoutingRequestDTO
    {
        public string RequestType { get; set; } = "Leave";
        public long EmployeeId { get; set; }
        public string? Department { get; set; }
        public string? Location { get; set; }
        public long? ProjectId { get; set; }
        public Dictionary<string, object> SampleContext { get; set; } = new();
    }

    public class TestRoutingStepResultDTO
    {
        public int StepOrder { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string ApproverRuleType { get; set; } = string.Empty;
        public bool ConditionMet { get; set; } = true;
        public string ConditionSummary { get; set; } = "Always applies";
        public List<long> EligibleApproverIds { get; set; } = new();
        public List<string> EligibleApproverNames { get; set; } = new();
        public string ResolutionReason { get; set; } = string.Empty;
        public string Mode { get; set; } = "AnyOne";
        public int? TimeLimitHours { get; set; }
        public string? EscalationTarget { get; set; }
    }

    public class TestRoutingResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string RequesterName { get; set; } = string.Empty;
        public string MatchedRouteScope { get; set; } = "Default";
        public List<TestRoutingStepResultDTO> Steps { get; set; } = new();
    }

    public class WorkflowVersionHistoryDTO
    {
        public int Version { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public int StepCount { get; set; }
    }
}
