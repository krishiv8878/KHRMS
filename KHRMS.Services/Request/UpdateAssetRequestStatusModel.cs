using System.ComponentModel.DataAnnotations;

namespace KHRMS.Services.Request
{
    public class UpdateAssetRequestStatusModel
    {
        [Required]
        public long RequestId { get; set; }

        [Required]
        [StringLength(50)]
        public string NewStatus { get; set; } = string.Empty;

        public long? ActionByEmployeeId { get; set; }

        [StringLength(150)]
        public string? ActionByName { get; set; }

        [StringLength(100)]
        public string? CourierPartner { get; set; }

        [StringLength(150)]
        public string? TrackingNumber { get; set; }

        [StringLength(1000)]
        public string? Remarks { get; set; }

        [StringLength(1000)]
        public string? AdminRemarks { get; set; }

        [StringLength(1000)]
        public string? InspectionRemarks { get; set; }
    }
}
