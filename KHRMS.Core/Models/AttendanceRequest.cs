using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace KHRMS.Core
{
    public class AttendanceRequest : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [ForeignKey("EmployeeId")]
        [Required(ErrorMessage = "Employee is required")]
        public long EmployeeId { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Request type is required")]
        public string? RequestType { get; set; }

        [Required(ErrorMessage = "Request Date type is required")]
        public DateTime RequestedDate { get; set; }

        [Required(ErrorMessage = "Request id type is required")]
        public long RequestedBy { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        public string? Reason { get; set; }

        public string? Status { get; set; }

        public long? LastActionBy { get; set; }

        [Required(ErrorMessage = "Clock In Time is required")]
        public DateTime ClockInTime { get; set; }

        public DateTime? ClockOutTime { get; set; }

        [ForeignKey("ManagerId")]
        [Required(ErrorMessage = "Manager Id is required")]
        public long ManagerId { get; set; }

        public virtual Employee Employee { get; set; }

    }
}
