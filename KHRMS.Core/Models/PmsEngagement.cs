using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("PmsFeedbacks")]
    public class PmsFeedback : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long FromEmployeeId { get; set; }

        [Required]
        public long ToEmployeeId { get; set; }

        [Required]
        [StringLength(50)]
        public string FeedbackType { get; set; } = "Praise"; // Praise, Constructive, 360 Peer, General

        [StringLength(100)]
        public string Badge { get; set; } = "Team Player"; // Customer Hero, Team Player, Problem Solver, Innovator, Rockstar

        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;

        public bool IsPrivate { get; set; } = false;

        public long? ReviewCycleId { get; set; }
    }

    [Table("PmsOneOnOnes")]
    public class PmsOneOnOne : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long ManagerId { get; set; }

        [Required]
        public long EmployeeId { get; set; }

        public DateTime ScheduledDate { get; set; }

        [Required]
        [StringLength(500)]
        public string Agenda { get; set; } = string.Empty;

        public string TalkingPointsJson { get; set; } = string.Empty;

        public string ActionItemsJson { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled
    }

    [Table("PmsPips")]
    public class PmsPip : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long EmployeeId { get; set; }

        [Required]
        public long ManagerId { get; set; }

        [Required]
        [StringLength(2000)]
        public string Reason { get; set; } = string.Empty;

        public int DurationDays { get; set; } = 30; // 30, 60, 90

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string ObjectivesJson { get; set; } = string.Empty;

        [StringLength(50)]
        public string CheckInFrequency { get; set; } = "Weekly";

        [Required]
        [StringLength(50)]
        public string Outcome { get; set; } = "Active"; // Active, Successful, Extended, Action Required

        public string FinalComments { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Active"; // Active, Closed
    }
}
