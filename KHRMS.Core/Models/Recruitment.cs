using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    public class JobRequisition : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string JobCode { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Department { get; set; } = "Engineering";

        [MaxLength(100)]
        public string Location { get; set; } = "Headquarters";

        [MaxLength(50)]
        public string EmploymentType { get; set; } = "Full-Time"; // Full-Time, Contract, Internship, Part-Time

        public decimal ExperienceMin { get; set; } = 0;
        public decimal ExperienceMax { get; set; } = 5;

        public decimal BudgetMin { get; set; } = 0;
        public decimal BudgetMax { get; set; } = 0;

        public int OpenPositions { get; set; } = 1;
        public int FilledPositions { get; set; } = 0;

        public string Description { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;

        public long? HiringManagerId { get; set; }
        public string? HiringManagerName { get; set; }

        public long? RecruiterId { get; set; }
        public string? RecruiterName { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Open"; // Draft, Open, On Hold, Closed

        public DateTime? TargetCloseDate { get; set; }
    }

    public class JobInterview : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long CandidateId { get; set; }

        public long? JobRequisitionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string RoundName { get; set; } = "Technical Round 1";

        public long InterviewerId { get; set; }

        [MaxLength(150)]
        public string InterviewerName { get; set; } = string.Empty;

        public DateTime ScheduledAt { get; set; }
        public int DurationMinutes { get; set; } = 45;

        public string? MeetingLink { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled, Rescheduled

        public int? Rating { get; set; } // 1 to 5

        [MaxLength(100)]
        public string? Recommendation { get; set; } // Strong Hire, Hire, Borderline, Reject

        public string? FeedbackNotes { get; set; }
        public DateTime? EvaluatedAt { get; set; }
    }

    public class JobOffer : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long CandidateId { get; set; }

        public long? JobRequisitionId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Designation { get; set; } = string.Empty;

        public decimal OfferedCtc { get; set; }
        public DateTime JoiningDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, Sent, Accepted, Declined, Revoked

        public string? Notes { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? IssuedDate { get; set; }
    }
}
