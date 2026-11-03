using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("ChecklistTemplates")]
    public class ChecklistTemplate : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ChecklistType { get; set; } = "Onboarding"; // "Onboarding" or "Offboarding"

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Department { get; set; } = "All";

        public virtual ICollection<ChecklistTemplateTask>? TemplateTasks { get; set; }
    }
}
