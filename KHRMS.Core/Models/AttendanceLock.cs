using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("AttendanceLocks")]
    public class AttendanceLock : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required(ErrorMessage = "Month is required")]
        public int Month { get; set; }

        [Required(ErrorMessage = "Year is required")]
        public int Year { get; set; }

        public int TotalEmployeesLocked { get; set; } = 0;
        public decimal TotalPresentDays { get; set; } = 0;
        public decimal TotalLopDays { get; set; } = 0;
        public decimal TotalOvertimeHours { get; set; } = 0;

        public bool IsLocked { get; set; } = true;

        public long LockedByEmployeeId { get; set; }
        public DateTime LockedDate { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? UnlockedReason { get; set; }

        public long? UnlockedByEmployeeId { get; set; }
        public DateTime? UnlockedDate { get; set; }
    }
}
