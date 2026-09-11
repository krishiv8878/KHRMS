using System.ComponentModel.DataAnnotations;

namespace KHRMS.Services.Request
{
    public class DocumentApprovalDTO
    {
        [Required]
        public long Id { get; set; }

        [Required]
        public string Status { get; set; } = "Approved";

        public string? RejectionReason { get; set; }
    }
}
