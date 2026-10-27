namespace KHRMS.Services.Workflow.Models
{
    public class WorkflowActionRequest
    {
        /// <summary>
        /// Action to perform: "Approve", "Reject", "OverrideApprove", "OverrideReject", "Reassign"
        /// </summary>
        public string Action { get; set; } = "Approve";

        /// <summary>
        /// Mandatory reason for Override / Reassign, or optional approval comment / rejection reason.
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// Target employee ID when reassigning a pending step.
        /// </summary>
        public long? ReassignToEmployeeId { get; set; }
    }
}
