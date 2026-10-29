using System;
using System.Collections.Generic;

namespace KHRMS.Services.Workflow.Models
{
    public class WorkflowInboxItemDTO
    {
        public long WorkflowInstanceId { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public long RequestId { get; set; }
        public long RequesterId { get; set; }
        public string RequesterName { get; set; } = string.Empty;
        public string? RequesterRole { get; set; }
        public string? RequesterDepartment { get; set; }
        public string? RequesterAvatar { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; }
        public DateTime? DueTime { get; set; }
        public int CurrentStepOrder { get; set; }
        public string CurrentStepName { get; set; } = string.Empty;
        public string PendingWith { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public bool IsEscalated { get; set; }

        // Server-computed capability flags
        public bool CanApprove { get; set; } = true;
        public bool CanOverride { get; set; } = false;
        public bool CanReassign { get; set; } = false;
        public bool CanCancel { get; set; } = false;

        public Dictionary<string, object?> PayloadSummary { get; set; } = new();
    }

    public class WorkflowScopedRequestDTO
    {
        public long WorkflowInstanceId { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public long RequestId { get; set; }
        public long RequesterId { get; set; }
        public string RequesterName { get; set; } = string.Empty;
        public string? RequesterDepartment { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; }
        public string Status { get; set; } = "Pending";
        public int CurrentStepOrder { get; set; }
        public string CurrentStepName { get; set; } = string.Empty;
        public string PendingWith { get; set; } = string.Empty;

        // Server-computed capability flags
        public bool CanApprove { get; set; }
        public bool CanOverride { get; set; }
        public bool CanReassign { get; set; }
        public bool CanCancel { get; set; }

        public Dictionary<string, object?> PayloadSummary { get; set; } = new();
    }

    public class WorkflowInstanceDetailDTO
    {
        public long WorkflowInstanceId { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public long RequestId { get; set; }
        public long RequesterId { get; set; }
        public string RequesterName { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public int DefinitionVersion { get; set; }
        public int CurrentStepOrder { get; set; }
        public DateTime StartedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string PendingWith { get; set; } = string.Empty;

        // Server-computed capability flags for current viewer
        public bool CanApprove { get; set; }
        public bool CanOverride { get; set; }
        public bool CanReassign { get; set; }
        public bool CanCancel { get; set; }

        public Dictionary<string, object?> PayloadSummary { get; set; } = new();

        public List<WorkflowStepDetailDTO> Steps { get; set; } = new();
        public List<WorkflowAuditItemDTO> AuditTrail { get; set; } = new();
    }

    public class WorkflowStepDetailDTO
    {
        public long Id { get; set; }
        public int StepOrder { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // "Pending", "Approved", "Rejected", "Waiting", "Skipped"
        public string ApproverRuleType { get; set; } = string.Empty;
        public string ResolutionReason { get; set; } = string.Empty;
        public List<string> EligibleApproverNames { get; set; } = new();
        public string? ActionByName { get; set; }
        public string? ActionCapacity { get; set; }
        public string? ActionDecision { get; set; }
        public string? Comment { get; set; }
        public DateTime StepStartTime { get; set; }
        public DateTime? ActionTime { get; set; }
        public DateTime? DueTime { get; set; }
        public bool IsEscalated { get; set; }
    }

    public class WorkflowAuditItemDTO
    {
        public long Id { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string PerformedByName { get; set; } = string.Empty;
        public string? ActionCapacity { get; set; }
        public string Details { get; set; } = string.Empty;
        public string? OldStatus { get; set; }
        public string? NewStatus { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
