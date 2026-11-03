using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core
{
    public class ShiftMaster : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string ShiftName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start Time is required")]
        public string? StartTime { get; set; } // e.g. "09:00"
        [Required(ErrorMessage = "End Time is required")]
        public string? EndTime { get; set; }   // e.g. "18:00"

        // Phase 4: Time & Attendance Policy Configuration
        public int GracePeriodMinutes { get; set; } = 15;
        public int FullDayMinimumMinutes { get; set; } = 480;  // 8 Hours
        public int HalfDayMinimumMinutes { get; set; } = 240;  // 4 Hours
        public int PresentMinimumMinutes { get; set; } = 120;  // 2 Hours
        public int ScheduledBreakMinutes { get; set; } = 60;   // 1 Hour
        public string? BreakStartTime { get; set; }            // e.g. "13:00"
        public string? BreakEndTime { get; set; }              // e.g. "14:00"
        public int MaxLateMarksPerMonth { get; set; } = 3;
        public string LateMarkPenalty { get; set; } = "HalfDayDeduction"; // "HalfDayDeduction", "LossOfPay", "Warning"
        public int AutoClockOutBufferHours { get; set; } = 4;
    }
}
