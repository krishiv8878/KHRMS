using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KHRMS.Core
{
    [Table("EmployeeAttendance")]
    public class EmployeeAttendance : KHRMSBase
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Employee Id is required")]
        public long EmployeeId { get; set; }

        [Required(ErrorMessage = "Clock In Time is required")]
        public DateTime ClockIn { get; set; }

        [Required(ErrorMessage = "Clock Out Time is required")]
        public DateTime ClockOut { get; set; }

        [Required(ErrorMessage = "Total Hours is required")]
        public DateTime TotalHours { get; set; } //  Changed from DateTime to TimeSpan

        [Required(ErrorMessage = "Gross Hours is required")]
        public DateTime EffectiveHours { get; set; }

        [StringLength(500)]
        public string? RegularizationReason { get; set; } // Reason provided by employee for irregularity

        public bool IsRegularized { get; set; } = false; // Indicates if manager has approved the regularization

        public DateTime? RegularizationRequestedDate { get; set; } // Timestamp when employee sent the request
    }
}

