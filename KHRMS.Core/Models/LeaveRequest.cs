
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
        public bool IsApproved { get; set; }  // Nullable
        public bool IsDeleted { get; set; }  // Nullable
        public bool IsActive { get; set; }  // Nullable
        public int ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }

        public string? LeaveReason { get; set; }
    }
}
