using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("ChecklistTemplateTasks")]
    public class ChecklistTemplateTask : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long TemplateId { get; set; }

        [ForeignKey("TemplateId")]
        public virtual ChecklistTemplate? Template { get; set; }

        [Required]
        [StringLength(200)]
        public string TaskName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = "Employee"; // "Employee", "HR", "IT", "Admin", "Finance", "Manager", "Facilities"

        [StringLength(50)]
        public string? DefaultAssigneeRole { get; set; }

        public long? DefaultAssigneeEmployeeId { get; set; }

        public int DueDays { get; set; } = 3;

        public bool IsMandatory { get; set; } = true;

        public int DisplayOrder { get; set; } = 1;
    }
}
