namespace KHRMS.Services.Workflow.Models
{
    public class ResolvedApproverItem
    {
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Capacity { get; set; } = "Approver"; // "Approver" or "Delegate"
        public long? DelegatedFromEmployeeId { get; set; }
        public string? DelegatedFromEmployeeName { get; set; }
        public string ResolutionReason { get; set; } = string.Empty;
    }
}
