using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("PolicyAcknowledgements")]
    public class PolicyAcknowledgement : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long PolicyId { get; set; }

        public int PolicyVersion { get; set; } = 1;

        [Required]
        public long EmployeeId { get; set; }

        public DateTime AcknowledgedDate { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string? IpAddress { get; set; }

        [ForeignKey("PolicyId")]
        public virtual CompanyPolicy? Policy { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }
    }
}
