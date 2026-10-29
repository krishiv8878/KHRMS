namespace KHRMS.Services.Workflow.Models
{
    public class ResolvedApproverCandidate
    {
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
