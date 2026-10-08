using System.ComponentModel.DataAnnotations;
using KHRMS.Core.Models;

namespace KHRMS.Services.Request
{
    public class CreateKeyResultDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal TargetValue { get; set; } = 100;
        public decimal CurrentValue { get; set; } = 0;
        public string MetricUnit { get; set; } = "%";
        public decimal Weightage { get; set; } = 100;
    }

    public class CreatePmsGoalRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public long EmployeeId { get; set; }
        public string Category { get; set; } = "Individual";
        public string Department { get; set; } = string.Empty;
        public string Period { get; set; } = "Q4 2026";
        public decimal Weightage { get; set; } = 100;
        public DateTime? DueDate { get; set; }
        public List<CreateKeyResultDto> KeyResults { get; set; } = new();
    }

    public class UpdatePmsGoalRequest
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "Individual";
        public string Department { get; set; } = string.Empty;
        public string Period { get; set; } = "Q4 2026";
        public decimal Weightage { get; set; } = 100;
        public int ProgressPercentage { get; set; }
        public string Status { get; set; } = "On Track";
        public DateTime? DueDate { get; set; }
        public List<CreateKeyResultDto> KeyResults { get; set; } = new();
    }

    public class PmsGoalCheckInRequest
    {
        public long GoalId { get; set; }
        public long? KeyResultId { get; set; }
        public decimal NewValue { get; set; }
        public int ConfidenceScore { get; set; } = 3;
        public string Notes { get; set; } = string.Empty;
    }

    public class PmsGoalDto
    {
        public PmsGoal Goal { get; set; } = null!;
        public List<PmsKeyResult> KeyResults { get; set; } = new();
        public List<PmsGoalCheckIn> CheckIns { get; set; } = new();
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
    }

    public class CreateReviewCycleRequest
    {
        [Required]
        public string CycleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Period { get; set; } = "Quarterly";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime SelfReviewDeadline { get; set; }
        public DateTime ManagerReviewDeadline { get; set; }
    }

    public class SubmitSelfReviewRequest
    {
        public long AppraisalId { get; set; }
        public decimal SelfRating { get; set; }
        public string SelfComments { get; set; } = string.Empty;
        public string CompetencyRatingsJson { get; set; } = string.Empty;
    }

    public class SubmitManagerReviewRequest
    {
        public long AppraisalId { get; set; }
        public decimal ManagerRating { get; set; }
        public string ManagerComments { get; set; } = string.Empty;
        public string CompetencyRatingsJson { get; set; } = string.Empty;
    }

    public class FinalizeAppraisalRequest
    {
        public long AppraisalId { get; set; }
        public decimal FinalRating { get; set; }
        public string HrComments { get; set; } = string.Empty;
    }

    public class PmsAppraisalDto
    {
        public PmsAppraisal Appraisal { get; set; } = null!;
        public PmsReviewCycle? Cycle { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
    }

    public class PmsFeedbackRequest
    {
        public long ToEmployeeId { get; set; }
        public string FeedbackType { get; set; } = "Praise";
        public string Badge { get; set; } = "Team Player";
        public string Message { get; set; } = string.Empty;
        public bool IsPrivate { get; set; } = false;
        public long? ReviewCycleId { get; set; }
    }

    public class PmsFeedbackDto
    {
        public PmsFeedback Feedback { get; set; } = null!;
        public string FromEmployeeName { get; set; } = string.Empty;
        public string FromEmployeeDesignation { get; set; } = string.Empty;
        public string ToEmployeeName { get; set; } = string.Empty;
        public string ToEmployeeDesignation { get; set; } = string.Empty;
    }

    public class PmsOneOnOneRequest
    {
        public long? Id { get; set; }
        public long EmployeeId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string Agenda { get; set; } = string.Empty;
        public string TalkingPointsJson { get; set; } = string.Empty;
        public string ActionItemsJson { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string Status { get; set; } = "Scheduled";
    }

    public class PmsOneOnOneDto
    {
        public PmsOneOnOne OneOnOne { get; set; } = null!;
        public string ManagerName { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeDesignation { get; set; } = string.Empty;
    }

    public class PmsPipRequest
    {
        public long? Id { get; set; }
        public long EmployeeId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public int DurationDays { get; set; } = 30;
        public DateTime StartDate { get; set; }
        public string ObjectivesJson { get; set; } = string.Empty;
        public string CheckInFrequency { get; set; } = "Weekly";
        public string Outcome { get; set; } = "Active";
        public string FinalComments { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
    }

    public class PmsPipDto
    {
        public PmsPip Pip { get; set; } = null!;
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
    }
}
