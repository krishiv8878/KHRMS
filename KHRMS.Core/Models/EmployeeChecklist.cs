using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("EmployeeChecklists")]
    public class EmployeeChecklist : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        public long? TemplateId { get; set; }

        [ForeignKey("TemplateId")]
        public virtual ChecklistTemplate? Template { get; set; }

        [Required]
        [StringLength(50)]
        public string ChecklistType { get; set; } = "Onboarding"; // "Onboarding" or "Offboarding"

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "InProgress"; // "Initiated", "InProgress", "Completed", "Cancelled"

        public int TotalTasks { get; set; } = 0;

        public int CompletedTasks { get; set; } = 0;

        public DateTime InitiatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedDate { get; set; }

        [StringLength(1000)]
        public string? Remarks { get; set; }

        public virtual ICollection<ChecklistTask>? Tasks { get; set; }
    }
}
