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

        [Required(ErrorMessage = "AttendanceDate required")]
        public DateOnly AttendanceDate { get; set; }

        [Required(ErrorMessage = "Clock In Time is required")]
        public DateTime ClockIn { get; set; }
        public DateTime? ClockOut { get; set; }

        [Required(ErrorMessage = "Total Hours is required") ]
        public decimal TotalHours { get; set; } //  Changed from DateTime to TimeSpan

        [Required(ErrorMessage = "Gross Hours is required")]
        public decimal EffectiveHours { get; set; }

    }
}

