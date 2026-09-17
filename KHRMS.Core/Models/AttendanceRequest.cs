using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace KHRMS.Core
{
    public class AttendanceRequest : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required(ErrorMessage = "Employee is required")]
        public long EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Request type is required")]
        public string? RequestType { get; set; }

        [Required(ErrorMessage = "Request Date type is required")]
        public DateTime RequestedDate { get; set; }

        [Required(ErrorMessage = "Request id type is required")]
        public long RequestedBy { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        public string? Reason { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "Pending";

        public long? LastActionBy { get; set; }

        public long? ActionBy { get; set; }

        public DateTime? ActionDate { get; set; }

        public string? RejectionReason { get; set; }

        [Required(ErrorMessage = "Clock In Time is required")]
        public DateTime ClockInTime { get; set; }

        public DateTime? ClockOutTime { get; set; }

        [Required(ErrorMessage = "Manager Id is required")]
        public long ManagerId { get; set; }

    }
}
