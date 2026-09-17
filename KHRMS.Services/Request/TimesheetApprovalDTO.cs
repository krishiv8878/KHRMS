using System.ComponentModel.DataAnnotations;

namespace KHRMS.Services.Request
{
    public class TimesheetApprovalDTO
    {
        [Required]
        public long TimesheetId { get; set; }

        [Required]
        public string Status { get; set; } = "Approved"; // "Approved" or "Rejected"

        public string? RejectionReason { get; set; }
    }
}
