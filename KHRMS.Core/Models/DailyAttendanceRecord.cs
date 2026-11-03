using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("DailyAttendanceRecords")]
    public class DailyAttendanceRecord : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required(ErrorMessage = "Employee is required")]
        public long EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }

        [Required(ErrorMessage = "Attendance date is required")]
        public DateOnly AttendanceDate { get; set; }

        public long? ShiftId { get; set; }
        [ForeignKey("ShiftId")]
        public ShiftMaster? Shift { get; set; }

        public DateTime? FirstClockIn { get; set; }
        public DateTime? LastClockOut { get; set; }

        public int TotalWorkMinutes { get; set; } = 0;
        public int TotalBreakMinutes { get; set; } = 0;
        public int EffectiveWorkMinutes { get; set; } = 0;
        public int OvertimeMinutes { get; set; } = 0;
        public int LateArrivalMinutes { get; set; } = 0;
        public int EarlyExitMinutes { get; set; } = 0;

        [StringLength(30)]
        public string Status { get; set; } = "Absent"; // "Present", "HalfDay", "Absent", "Late", "OnLeave", "Holiday", "WeekOff", "WFH", "MissedPunch"

        public long? LeaveRequestId { get; set; }

        public bool IsRegularized { get; set; } = false;

        public long? AttendanceLockId { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}
