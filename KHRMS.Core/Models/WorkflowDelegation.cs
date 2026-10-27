using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("WorkflowDelegations")]
    public class WorkflowDelegation : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long FromEmployeeId { get; set; }

        [ForeignKey("FromEmployeeId")]
        public virtual Employee? FromEmployee { get; set; }

        [Required]
        public long ToEmployeeId { get; set; }

        [ForeignKey("ToEmployeeId")]
        public virtual Employee? ToEmployee { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [StringLength(100)]
        public string? RequestType { get; set; } // Null or "All" applies to all request types; or e.g. "Leave"

        [StringLength(500)]
        public string? Reason { get; set; }
    }
}
