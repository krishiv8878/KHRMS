using System.ComponentModel.DataAnnotations;

namespace KHRMS.Services.Request
{
    public class CreateAssetRequestModel
    {
        [Required]
        public long AssetId { get; set; }

        [Required]
        public long EmployeeId { get; set; }

        [Required]
        [StringLength(50)]
        public string RequestType { get; set; } = "Repair"; // "Repair", "Replacement", "Return"

        [StringLength(50)]
        public string? Priority { get; set; } = "Medium";

        [Required]
        [StringLength(200)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(1500)]
        public string? Description { get; set; }

        public string? ImageUrls { get; set; } // JSON array string or comma-separated URLs
    }
}
