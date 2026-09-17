
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KHRMS.Core
{

    public class LeaveRequest : KHRMSBase
    {
        [Key]
        public long Id { get; set; }  // Primary Key      
        [ForeignKey("Employee")]
        public long EmployeeId { get; set; }
        public virtual Employee? Employee { get; set; }

        [ForeignKey("LeaveType")]
        public long LeaveTypeId { get; set; }
        public virtual LeaveType? LeaveType { get; set;}
        public string? LeaveMode { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }

        public string? LeaveReason { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "Pending";

        public long? ActionBy { get; set; }
        public DateTime? ActionDate { get; set; }

        public string? RejectionReason { get; set; }
    }
}
