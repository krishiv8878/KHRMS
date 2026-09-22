using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core
{
    public class AssetRequest : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long AssetId { get; set; }

        [ForeignKey("AssetId")]
        public virtual AssetsMaster? Asset { get; set; }

        [Required]
        public long EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        [Required]
        [StringLength(50)]
        public string RequestType { get; set; } = "Repair"; // "Repair", "Replacement", "Return"

        [StringLength(50)]
        public string? Priority { get; set; } = "Medium"; // "Low", "Medium", "High", "Critical"

        [StringLength(200)]
        public string? Reason { get; set; }

        [StringLength(1500)]
        public string? Description { get; set; }

        // JSON array or comma-separated image file paths / URLs
        public string? ImageUrls { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Submitted"; 
        // "Submitted", "Approved", "In Repair", "Repair Completed", "Dispatched", "Delivered", "Received", "Pending Inspection", "Completed", "Rejected"

        [StringLength(100)]
        public string? CourierPartner { get; set; }

        [StringLength(150)]
        public string? TrackingNumber { get; set; }

        public DateTime? DispatchedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? ReceivedDate { get; set; }

        [StringLength(1000)]
        public string? AdminRemarks { get; set; }

        [StringLength(1000)]
        public string? InspectionRemarks { get; set; }

        public virtual ICollection<AssetRequestLog> Logs { get; set; } = new List<AssetRequestLog>();
    }
}
