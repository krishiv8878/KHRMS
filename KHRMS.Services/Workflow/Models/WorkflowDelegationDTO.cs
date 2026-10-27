using System;

namespace KHRMS.Services.Workflow.Models
{
    public class WorkflowDelegationDTO
    {
        public long Id { get; set; }
        public long FromEmployeeId { get; set; }
        public string? FromEmployeeName { get; set; }
        public long ToEmployeeId { get; set; }
        public string? ToEmployeeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? RequestType { get; set; } // Null / "All" or "Leave"
        public string? Reason { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateWorkflowDelegationRequest
    {
        public long ToEmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? RequestType { get; set; }
        public string? Reason { get; set; }
    }
}
