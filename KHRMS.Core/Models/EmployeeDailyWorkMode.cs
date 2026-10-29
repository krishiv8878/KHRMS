using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("EmployeeDailyWorkModes")]
    public class EmployeeDailyWorkMode : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required(ErrorMessage = "Employee Id is required")]
        public long EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        [Required(ErrorMessage = "Work Date is required")]
        public DateOnly WorkDate { get; set; }

        [Required(ErrorMessage = "Work Mode is required")]
        [StringLength(20)]
        public string WorkMode { get; set; } = "WFO"; // WFO, WFH, OD

        [Required(ErrorMessage = "Selected By is required")]
        [StringLength(50)]
        public string SelectedBy { get; set; } = "Employee"; // Employee, Manager, Admin, Policy

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
