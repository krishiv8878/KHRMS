using System.ComponentModel.DataAnnotations;

namespace KHRMS.Services.Request
{
    public class TimesheetSubmitDTO
    {
        [Required]
        public string PeriodType { get; set; } = "Weekly"; // "Weekly" or "Monthly"

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public decimal? EstimatedActiveShiftHours { get; set; } // Option 1 default shift hours if currently clocked-in
    }
}
