using System;
using System.Collections.Generic;

namespace KHRMS.Services.Request
{
    public class JobRequisitionDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string JobCode { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = string.Empty;
        public decimal ExperienceMin { get; set; }
        public decimal ExperienceMax { get; set; }
        public decimal BudgetMin { get; set; }
        public decimal BudgetMax { get; set; }
        public int OpenPositions { get; set; }
        public int FilledPositions { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public long? HiringManagerId { get; set; }
        public string? HiringManagerName { get; set; }
        public long? RecruiterId { get; set; }
        public string? RecruiterName { get; set; }
        public string Status { get; set; } = "Open";
        public DateTime? TargetCloseDate { get; set; }
        public DateTime CreatedAt { get; set; }

        // Pipeline counts
        public int TotalApplicants { get; set; }
        public int SourcedCount { get; set; }
        public int ScreeningCount { get; set; }
        public int InterviewCount { get; set; }
        public int OfferedCount { get; set; }
        public int HiredCount { get; set; }
        public int RejectedCount { get; set; }
    }

    public class CreateJobRequisitionRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Department { get; set; } = "Engineering";
        public string Location { get; set; } = "Headquarters";
        public string EmploymentType { get; set; } = "Full-Time";
        public decimal ExperienceMin { get; set; }
        public decimal ExperienceMax { get; set; }
        public decimal BudgetMin { get; set; }
        public decimal BudgetMax { get; set; }
        public int OpenPositions { get; set; } = 1;
        public string Description { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public long? HiringManagerId { get; set; }
        public long? RecruiterId { get; set; }
        public DateTime? TargetCloseDate { get; set; }
    }

    public class CandidatePipelineItemDto
    {
        public long Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string EmailAddress { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string TotalExperience { get; set; } = string.Empty;
        public string RelevantExperience { get; set; } = string.Empty;
        public long? CurrentSalary { get; set; }
        public long? ExpectedSalary { get; set; }
        public int? NoticePeriod { get; set; }
        public string AppliedRole { get; set; } = string.Empty;
        public string Stage { get; set; } = "Sourced";
        public int? MatchScore { get; set; }
        public long? JobRequisitionId { get; set; }
        public string? JobRequisitionTitle { get; set; }
        public string? JobCode { get; set; }
        public string? ResumeUrl { get; set; }
        public string? Source { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<JobInterviewDto> Interviews { get; set; } = new();
        public JobOfferDto? LatestOffer { get; set; }
    }

    public class UpdateCandidateStageRequest
    {
        public long CandidateId { get; set; }
        public string NewStage { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class JobInterviewDto
    {
        public long Id { get; set; }
        public long CandidateId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string CandidateEmail { get; set; } = string.Empty;
        public string CandidateRole { get; set; } = string.Empty;
        public long? JobRequisitionId { get; set; }
        public string? JobTitle { get; set; }
        public string RoundName { get; set; } = string.Empty;
        public long InterviewerId { get; set; }
        public string InterviewerName { get; set; } = string.Empty;
        public DateTime ScheduledAt { get; set; }
        public int DurationMinutes { get; set; }
        public string? MeetingLink { get; set; }
        public string Status { get; set; } = "Scheduled";
        public int? Rating { get; set; }
        public string? Recommendation { get; set; }
        public string? FeedbackNotes { get; set; }
        public DateTime? EvaluatedAt { get; set; }
    }

    public class ScheduleInterviewRequest
    {
        public long CandidateId { get; set; }
        public long? JobRequisitionId { get; set; }
        public string RoundName { get; set; } = "Technical Interview 1";
        public long InterviewerId { get; set; }
        public DateTime ScheduledAt { get; set; }
        public int DurationMinutes { get; set; } = 45;
        public string? MeetingLink { get; set; }
    }

    public class SubmitInterviewFeedbackRequest
    {
        public long InterviewId { get; set; }
        public int Rating { get; set; } // 1-5
        public string Recommendation { get; set; } = "Hire"; // Strong Hire, Hire, Borderline, Reject
        public string FeedbackNotes { get; set; } = string.Empty;
        public bool AdvanceToNextStage { get; set; } = true;
    }

    public class JobOfferDto
    {
        public long Id { get; set; }
        public long CandidateId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string CandidateEmail { get; set; } = string.Empty;
        public long? JobRequisitionId { get; set; }
        public string? JobTitle { get; set; }
        public string Designation { get; set; } = string.Empty;
        public decimal OfferedCtc { get; set; }
        public DateTime JoiningDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, Sent, Accepted, Declined, Revoked
        public string? Notes { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? IssuedDate { get; set; }
        public bool IsCandidateOnboarded { get; set; } = false;
    }

    public class CreateJobOfferRequest
    {
        public long CandidateId { get; set; }
        public long? JobRequisitionId { get; set; }
        public string Designation { get; set; } = string.Empty;
        public decimal OfferedCtc { get; set; }
        public DateTime JoiningDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Notes { get; set; }
    }

    public class RecruitmentDashboardMetricsDto
    {
        public int OpenRequisitionsCount { get; set; }
        public int TotalCandidatesInPipeline { get; set; }
        public int ActiveInterviewsScheduled { get; set; }
        public int OffersReleasedCount { get; set; }
        public int OffersAcceptedCount { get; set; }
        public int HiredThisQuarterCount { get; set; }
    }
}
