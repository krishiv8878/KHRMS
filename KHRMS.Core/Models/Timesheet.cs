using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("Timesheets")]
    public class Timesheet : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long EmployeeId { get; set; }

        [Required]
        [StringLength(20)]
        public string PeriodType { get; set; } = "Weekly"; // "Weekly" or "Monthly"

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal TotalHours { get; set; } = 0;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Draft"; // "Draft", "Submitted", "Approved", "Rejected"

        public DateTime? SubmittedDate { get; set; }

        public long? ManagerId { get; set; }

        public long? ActionBy { get; set; }

        public DateTime? ActionDate { get; set; }

        public string? RejectionReason { get; set; }
    }
}
