namespace KHRMS.Services.Workflow.Models
{
    public class WorkflowActionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string StatusCode { get; set; } = "OK"; // "OK", "AlreadyActioned", "Forbidden", "NotFound", "BadRequest"
        public string WorkflowStatus { get; set; } = "Pending"; // "Pending", "Approved", "Rejected", "Cancelled"
        public bool IsWorkflowCompleted { get; set; }
        public int CurrentStepOrder { get; set; }
        public string? ActionCapacity { get; set; } // "Approver", "Delegate", "Override"
    }
}
