using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("PmsReviewCycles")]
    public class PmsReviewCycle : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(150)]
        public string CycleName { get; set; } = string.Empty; // e.g. "Annual Appraisal 2026", "Q3 2026 Review"

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Period { get; set; } = "Quarterly"; // Annual, Half-Yearly, Quarterly

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime SelfReviewDeadline { get; set; }

        public DateTime ManagerReviewDeadline { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Active"; // Draft, Active, Evaluation, Completed, Closed
    }

    [Table("PmsAppraisals")]
    public class PmsAppraisal : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long ReviewCycleId { get; set; }

        [Required]
        public long EmployeeId { get; set; }

        public long? ManagerId { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal? SelfRating { get; set; } // 1.00 to 5.00

        public string SelfComments { get; set; } = string.Empty;

        public DateTime? SelfSubmittedDate { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal? ManagerRating { get; set; } // 1.00 to 5.00

        public string ManagerComments { get; set; } = string.Empty;

        public DateTime? ManagerSubmittedDate { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal? FinalRating { get; set; } // 1.00 to 5.00

        public string HrComments { get; set; } = string.Empty;

        [Required]
        [StringLength(40)]
        public string Status { get; set; } = "Self Review Pending"; // Self Review Pending, Manager Review Pending, HR Review Pending, Completed

        public string CompetencyRatingsJson { get; set; } = string.Empty; // JSON array of competencies with ratings & remarks

        [Column(TypeName = "decimal(5,2)")]
        public decimal? GoalsScore { get; set; } // 0 to 100 derived from OKRs
    }
}
