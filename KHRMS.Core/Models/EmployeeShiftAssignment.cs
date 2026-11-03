using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("EmployeeShiftAssignments")]
    public class EmployeeShiftAssignment : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required(ErrorMessage = "Employee is required")]
        public long EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }

        [Required(ErrorMessage = "Shift is required")]
        public long ShiftId { get; set; }
        [ForeignKey("ShiftId")]
        public ShiftMaster? Shift { get; set; }

        [Required(ErrorMessage = "Effective From Date is required")]
        public DateOnly EffectiveFromDate { get; set; }

        public DateOnly? EffectiveToDate { get; set; }

        [StringLength(100)]
        public string WeekOffPattern { get; set; } = "Sunday,Saturday"; // e.g. "Sunday,Saturday", "Sunday", "AlternateSaturday"
    }
}
