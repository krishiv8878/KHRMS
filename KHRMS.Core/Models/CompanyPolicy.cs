using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("CompanyPolicies")]
    public class CompanyPolicy : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = "General"; // 'Leave', 'Conduct', 'Attendance', 'IT & Security', 'Compensation', 'General'

        [Required]
        public string ContentHtml { get; set; } = string.Empty;

        public int Version { get; set; } = 1;

        public bool IsMandatoryAcknowledgement { get; set; } = true;

        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow.Date;
    }
}
