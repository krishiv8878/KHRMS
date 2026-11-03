using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("ChecklistTasks")]
    public class ChecklistTask : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long EmployeeChecklistId { get; set; }

        [ForeignKey("EmployeeChecklistId")]
        public virtual EmployeeChecklist? EmployeeChecklist { get; set; }

        [Required]
        [StringLength(200)]
        public string TaskName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = "Employee"; // "Employee", "HR", "IT", "Admin", "Finance", "Manager", "Facilities"

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // "Pending", "InProgress", "Completed", "Waived", "Rejected"

        public DateTime? DueDate { get; set; }

        public long? AssignedEmployeeId { get; set; }

        [ForeignKey("AssignedEmployeeId")]
        public virtual Employee? AssignedEmployee { get; set; }

        [StringLength(50)]
        public string? AssignedRole { get; set; }

        public bool IsMandatory { get; set; } = true;

        public DateTime? ClearedDate { get; set; }

        public long? ClearedByEmployeeId { get; set; }

        [ForeignKey("ClearedByEmployeeId")]
        public virtual Employee? ClearedByEmployee { get; set; }

        [StringLength(1000)]
        public string? Remarks { get; set; }

        [StringLength(1000)]
        public string? WaiveReason { get; set; }

        public long? WaivedByEmployeeId { get; set; }

        [ForeignKey("WaivedByEmployeeId")]
        public virtual Employee? WaivedByEmployee { get; set; }

        [StringLength(500)]
        public string? AttachmentUrl { get; set; }

        public int DisplayOrder { get; set; } = 1;
    }
}
