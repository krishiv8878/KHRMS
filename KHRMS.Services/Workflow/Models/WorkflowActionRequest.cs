namespace KHRMS.Services.Workflow.Models
{
    public class WorkflowActionRequest
    {
        /// <summary>
        /// Action to perform: "Approve", "Reject", "ReturnForRevision", "OverrideApprove", "OverrideReject", "Reassign"
        /// </summary>
        public string Action { get; set; } = "Approve";

        /// <summary>
        /// Mandatory reason for Override / Reassign / ReturnForRevision, or optional approval comment / rejection reason.
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// Target employee ID when reassigning a pending step.
        /// </summary>
        public long? ReassignToEmployeeId { get; set; }
    }

    public class WorkflowResubmitRequest
    {
        public string? RequestType { get; set; }
        public long? RequestId { get; set; }
        public string? Comment { get; set; }
    }
}
