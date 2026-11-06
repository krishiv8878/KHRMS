using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("Departments")]
    public class Department : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required(ErrorMessage = "Department Name is required")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Code { get; set; }

        [StringLength(50)]
        public string? CostCenter { get; set; }

        public long? ManagerId { get; set; }

        [ForeignKey("ManagerId")]
        public virtual Employee? Manager { get; set; }

        [StringLength(250)]
        public string? Description { get; set; }
    }
}
