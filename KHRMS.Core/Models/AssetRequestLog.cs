using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core
{
    public class AssetRequestLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long AssetRequestId { get; set; }

        [ForeignKey("AssetRequestId")]
        public virtual AssetRequest? AssetRequest { get; set; }

        [StringLength(50)]
        public string? FromStatus { get; set; }

        [Required]
        [StringLength(50)]
        public string ToStatus { get; set; } = string.Empty;

        public long? ActionByEmployeeId { get; set; }

        [StringLength(150)]
        public string? ActionByName { get; set; }

        [StringLength(1000)]
        public string? Remarks { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
