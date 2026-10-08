using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace KHRMS.Services
{
    public class RecruitmentService : IRecruitmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly KHRMSContextClass _context;
        private readonly IUserContextService _userContext;
        private static bool _tablesVerified = false;
        private static readonly object _lock = new();

        public RecruitmentService(IUnitOfWork unitOfWork, KHRMSContextClass context, IUserContextService userContext)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _userContext = userContext;
        }


        // ==========================================
        // 1. JOB REQUISITIONS
        // ==========================================
        public async Task<List<JobRequisitionDto>> GetAllRequisitions(string? status = null)
        {
            var query = _context.JobRequisitions.Where(r => !r.IsDeleted);
            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(r => r.Status == status);
            }

            var list = await query.OrderByDescending(r => r.CreatedDate).ToListAsync();
            var candidates = await _context.Candidates.Where(c => !c.IsDeleted).ToListAsync();

            var result = new List<JobRequisitionDto>();
            foreach (var r in list)
            {
                var candForReq = candidates.Where(c => c.JobRequisitionId == r.Id || 
                    (!string.IsNullOrEmpty(c.AppliedRole) && c.AppliedRole.Contains(r.Title, StringComparison.OrdinalIgnoreCase))).ToList();

                result.Add(new JobRequisitionDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    JobCode = r.JobCode,
                    Department = r.Department,
                    Location = r.Location,
                    EmploymentType = r.EmploymentType,
                    ExperienceMin = r.ExperienceMin,
                    ExperienceMax = r.ExperienceMax,
                    BudgetMin = r.BudgetMin,
                    BudgetMax = r.BudgetMax,
                    OpenPositions = r.OpenPositions,
                    FilledPositions = r.FilledPositions,
                    Description = r.Description,
                    Requirements = r.Requirements,
                    HiringManagerId = r.HiringManagerId,
                    HiringManagerName = r.HiringManagerName,
                    RecruiterId = r.RecruiterId,
                    RecruiterName = r.RecruiterName,
                    Status = r.Status,
                    TargetCloseDate = r.TargetCloseDate,
                    CreatedAt = r.CreatedDate ?? DateTime.UtcNow,
                    TotalApplicants = candForReq.Count,
                    SourcedCount = candForReq.Count(c => c.Stage == "Sourced" || string.IsNullOrEmpty(c.Stage)),
                    ScreeningCount = candForReq.Count(c => c.Stage == "Screening"),
                    InterviewCount = candForReq.Count(c => c.Stage == "Interview" || c.Stage?.StartsWith("Round") == true),
                    OfferedCount = candForReq.Count(c => c.Stage == "Offered"),
                    HiredCount = candForReq.Count(c => c.Stage == "Hired" || c.Stage == "Onboarded"),
                    RejectedCount = candForReq.Count(c => c.Stage == "Rejected")
                });
            }

            return result;
        }

        public async Task<JobRequisitionDto?> GetRequisitionById(long id)
        {
            var all = await GetAllRequisitions();
            return all.FirstOrDefault(r => r.Id == id);
        }

        public async Task<JobRequisitionDto> CreateRequisition(CreateJobRequisitionRequest request)
        {
            var empId = _userContext.GetCurrentEmployeeId();
            var count = await _context.JobRequisitions.CountAsync() + 1;
            var code = $"REQ-{DateTime.UtcNow.Year}-{count:D3}";

            var hiringManager = request.HiringManagerId.HasValue 
                ? await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.HiringManagerId.Value) 
                : null;
            var recruiter = request.RecruiterId.HasValue 
                ? await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.RecruiterId.Value) 
                : null;

            var entity = new JobRequisition
            {
                Title = request.Title,
                JobCode = code,
                Department = request.Department,
                Location = request.Location,
                EmploymentType = request.EmploymentType,
                ExperienceMin = request.ExperienceMin,
                ExperienceMax = request.ExperienceMax,
                BudgetMin = request.BudgetMin,
                BudgetMax = request.BudgetMax,
                OpenPositions = request.OpenPositions > 0 ? request.OpenPositions : 1,
                FilledPositions = 0,
                Description = request.Description,
                Requirements = request.Requirements,
                HiringManagerId = request.HiringManagerId,
                HiringManagerName = hiringManager != null ? $"{hiringManager.FirstName} {hiringManager.LastName}".Trim() : null,
                RecruiterId = request.RecruiterId,
                RecruiterName = recruiter != null ? $"{recruiter.FirstName} {recruiter.LastName}".Trim() : null,
                Status = "Open",
                TargetCloseDate = request.TargetCloseDate,
                CreatedBy = (int)(empId > 0 ? empId : 1),
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.JobRequisitions.Add(entity);
            _unitOfWork.Save();

            return (await GetRequisitionById(entity.Id))!;
        }

        public async Task<JobRequisitionDto> UpdateRequisition(long id, CreateJobRequisitionRequest request)
        {
            var entity = await _unitOfWork.JobRequisitions.GetById(id)
                ?? throw new KeyNotFoundException($"Job Requisition with ID {id} not found");

            var empId = _userContext.GetCurrentEmployeeId();
            var hiringManager = request.HiringManagerId.HasValue 
                ? await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.HiringManagerId.Value) 
                : null;
            var recruiter = request.RecruiterId.HasValue 
                ? await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.RecruiterId.Value) 
                : null;

            entity.Title = request.Title;
            entity.Department = request.Department;
            entity.Location = request.Location;
            entity.EmploymentType = request.EmploymentType;
            entity.ExperienceMin = request.ExperienceMin;
            entity.ExperienceMax = request.ExperienceMax;
            entity.BudgetMin = request.BudgetMin;
            entity.BudgetMax = request.BudgetMax;
            entity.OpenPositions = request.OpenPositions;
            entity.Description = request.Description;
            entity.Requirements = request.Requirements;
            entity.HiringManagerId = request.HiringManagerId;
            entity.HiringManagerName = hiringManager != null ? $"{hiringManager.FirstName} {hiringManager.LastName}".Trim() : entity.HiringManagerName;
            entity.RecruiterId = request.RecruiterId;
            entity.RecruiterName = recruiter != null ? $"{recruiter.FirstName} {recruiter.LastName}".Trim() : entity.RecruiterName;
            entity.TargetCloseDate = request.TargetCloseDate;
            entity.UpdatedBy = (int)(empId > 0 ? empId : 1);
            entity.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.JobRequisitions.Update(entity);
            _unitOfWork.Save();

            return (await GetRequisitionById(entity.Id))!;
        }

        public async Task<bool> UpdateRequisitionStatus(long id, string status)
        {
            var entity = await _unitOfWork.JobRequisitions.GetById(id);
            if (entity == null) return false;

            entity.Status = status;
            entity.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.JobRequisitions.Update(entity);
            _unitOfWork.Save();
            return true;
        }

        public async Task<bool> DeleteRequisition(long id)
        {
            var entity = await _unitOfWork.JobRequisitions.GetById(id);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.JobRequisitions.Update(entity);
            _unitOfWork.Save();
            return true;
        }

        // ==========================================
        // 2. CANDIDATES PIPELINE
        // ==========================================
        public async Task<List<CandidatePipelineItemDto>> GetPipelineCandidates(long? requisitionId = null, string? stage = null)
        {
            var cands = await _context.Candidates.Where(c => !c.IsDeleted).ToListAsync();
            var reqs = await _context.JobRequisitions.Where(r => !r.IsDeleted).ToListAsync();
            var interviews = await _context.JobInterviews.Where(i => !i.IsDeleted).OrderByDescending(i => i.ScheduledAt).ToListAsync();
            var offers = await _context.JobOffers.Where(o => !o.IsDeleted).OrderByDescending(o => o.CreatedDate).ToListAsync();

            if (requisitionId.HasValue)
            {
                var req = reqs.FirstOrDefault(r => r.Id == requisitionId.Value);
                cands = cands.Where(c => c.JobRequisitionId == requisitionId.Value ||
                    (req != null && !string.IsNullOrEmpty(c.AppliedRole) && c.AppliedRole.Contains(req.Title, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            if (!string.IsNullOrWhiteSpace(stage) && stage != "All")
            {
                cands = cands.Where(c => string.Equals(c.Stage, stage, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var result = new List<CandidatePipelineItemDto>();
            foreach (var c in cands)
            {
                var matchedReq = reqs.FirstOrDefault(r => r.Id == c.JobRequisitionId) 
                    ?? reqs.FirstOrDefault(r => !string.IsNullOrEmpty(c.AppliedRole) && c.AppliedRole.Contains(r.Title, StringComparison.OrdinalIgnoreCase));

                var candInterviews = interviews.Where(i => i.CandidateId == c.Id).Select(i => new JobInterviewDto
                {
                    Id = i.Id,
                    CandidateId = i.CandidateId,
                    CandidateName = $"{c.FirstName} {c.LastName}".Trim(),
                    CandidateEmail = c.EmailAddress ?? string.Empty,
                    CandidateRole = c.AppliedRole ?? string.Empty,
                    JobRequisitionId = i.JobRequisitionId,
                    JobTitle = matchedReq?.Title,
                    RoundName = i.RoundName,
                    InterviewerId = i.InterviewerId,
                    InterviewerName = i.InterviewerName,
                    ScheduledAt = i.ScheduledAt,
                    DurationMinutes = i.DurationMinutes,
                    MeetingLink = i.MeetingLink,
                    Status = i.Status,
                    Rating = i.Rating,
                    Recommendation = i.Recommendation,
                    FeedbackNotes = i.FeedbackNotes,
                    EvaluatedAt = i.EvaluatedAt
                }).ToList();

                var latestOffer = offers.Where(o => o.CandidateId == c.Id).Select(o => new JobOfferDto
                {
                    Id = o.Id,
                    CandidateId = o.CandidateId,
                    CandidateName = $"{c.FirstName} {c.LastName}".Trim(),
                    CandidateEmail = c.EmailAddress ?? string.Empty,
                    JobRequisitionId = o.JobRequisitionId,
                    JobTitle = matchedReq?.Title,
                    Designation = o.Designation,
                    OfferedCtc = o.OfferedCtc,
                    JoiningDate = o.JoiningDate,
                    ExpiryDate = o.ExpiryDate,
                    Status = o.Status,
                    Notes = o.Notes,
                    ApprovedBy = o.ApprovedBy,
                    IssuedDate = o.IssuedDate
                }).FirstOrDefault();

                result.Add(new CandidatePipelineItemDto
                {
                    Id = c.Id,
                    FirstName = c.FirstName ?? string.Empty,
                    LastName = c.LastName ?? string.Empty,
                    EmailAddress = c.EmailAddress ?? string.Empty,
                    MobileNumber = c.MobileNumber ?? string.Empty,
                    TotalExperience = c.TotalExperience ?? string.Empty,
                    RelevantExperience = c.RelevantExperience ?? string.Empty,
                    CurrentSalary = c.CurrentSalary,
                    ExpectedSalary = c.ExpectedSalary,
                    NoticePeriod = c.NoticePeriod,
                    AppliedRole = c.AppliedRole ?? string.Empty,
                    Stage = string.IsNullOrWhiteSpace(c.Stage) ? "Sourced" : c.Stage,
                    MatchScore = c.MatchScore,
                    JobRequisitionId = matchedReq?.Id,
                    JobRequisitionTitle = matchedReq?.Title,
                    JobCode = matchedReq?.JobCode,
                    ResumeUrl = c.ResumeUrl,
                    Source = c.Source ?? "Direct Applied",
                    CreatedAt = c.CreatedDate ?? DateTime.UtcNow,
                    Interviews = candInterviews,
                    LatestOffer = latestOffer
                });
            }

            return result.OrderByDescending(c => c.CreatedAt).ToList();
        }

        public async Task<CandidatePipelineItemDto?> GetCandidatePipelineDetail(long candidateId)
        {
            var list = await GetPipelineCandidates();
            return list.FirstOrDefault(c => c.Id == candidateId);
        }

        public async Task<bool> UpdateCandidateStage(UpdateCandidateStageRequest request)
        {
            var cand = await _context.Candidates.FirstOrDefaultAsync(c => c.Id == request.CandidateId);
            if (cand == null) return false;

            cand.Stage = request.NewStage;
            cand.UpdatedDate = DateTime.UtcNow;
            _context.Candidates.Update(cand);
            await _context.SaveChangesAsync();
            return true;
        }

        // ==========================================
        // 3. INTERVIEWS
        // ==========================================
        public async Task<List<JobInterviewDto>> GetInterviews(long? candidateId = null, long? interviewerId = null, string? status = null)
        {
            var query = _context.JobInterviews.Where(i => !i.IsDeleted);
            if (candidateId.HasValue) query = query.Where(i => i.CandidateId == candidateId.Value);
            if (interviewerId.HasValue) query = query.Where(i => i.InterviewerId == interviewerId.Value);
            if (!string.IsNullOrWhiteSpace(status) && status != "All") query = query.Where(i => i.Status == status);

            var interviews = await query.OrderByDescending(i => i.ScheduledAt).ToListAsync();
            var candIds = interviews.Select(i => i.CandidateId).Distinct().ToList();
            var candidates = await _context.Candidates.Where(c => candIds.Contains(c.Id)).ToListAsync();
            var reqs = await _context.JobRequisitions.Where(r => !r.IsDeleted).ToListAsync();

            return interviews.Select(i =>
            {
                var cand = candidates.FirstOrDefault(c => c.Id == i.CandidateId);
                var req = reqs.FirstOrDefault(r => r.Id == i.JobRequisitionId);
                return new JobInterviewDto
                {
                    Id = i.Id,
                    CandidateId = i.CandidateId,
                    CandidateName = cand != null ? $"{cand.FirstName} {cand.LastName}".Trim() : string.Empty,
                    CandidateEmail = cand?.EmailAddress ?? string.Empty,
                    CandidateRole = cand?.AppliedRole ?? string.Empty,
                    JobRequisitionId = i.JobRequisitionId,
                    JobTitle = req?.Title,
                    RoundName = i.RoundName,
                    InterviewerId = i.InterviewerId,
                    InterviewerName = i.InterviewerName,
                    ScheduledAt = i.ScheduledAt,
                    DurationMinutes = i.DurationMinutes,
                    MeetingLink = i.MeetingLink,
                    Status = i.Status,
                    Rating = i.Rating,
                    Recommendation = i.Recommendation,
                    FeedbackNotes = i.FeedbackNotes,
                    EvaluatedAt = i.EvaluatedAt
                };
            }).ToList();
        }

        public async Task<JobInterviewDto> ScheduleInterview(ScheduleInterviewRequest request)
        {
            var empId = _userContext.GetCurrentEmployeeId();

            // 1. Check if candidate already has an active (Scheduled/Rescheduled) interview in progress
            var activeInterview = await _context.JobInterviews
                .FirstOrDefaultAsync(i => i.CandidateId == request.CandidateId && !i.IsDeleted && (i.Status == "Scheduled" || i.Status == "Rescheduled"));

            if (activeInterview != null)
            {
                throw new InvalidOperationException($"An active interview '{activeInterview.RoundName}' is already scheduled for this candidate with {activeInterview.InterviewerName} on {activeInterview.ScheduledAt:g}. Please submit the evaluation scorecard or cancel the current round before booking another round.");
            }

            // 2. Check if candidate has already completed and passed this exact round
            var passedRound = await _context.JobInterviews
                .FirstOrDefaultAsync(i => i.CandidateId == request.CandidateId && !i.IsDeleted && i.Status == "Completed" && i.RoundName == request.RoundName && i.Recommendation != "Reject");

            if (passedRound != null)
            {
                throw new InvalidOperationException($"Candidate has already completed and passed '{request.RoundName}'. Please select the next progression round.");
            }

            var interviewer = await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.InterviewerId)
                ?? throw new KeyNotFoundException($"Interviewer with ID {request.InterviewerId} not found");

            var entity = new JobInterview
            {
                CandidateId = request.CandidateId,
                JobRequisitionId = request.JobRequisitionId,
                RoundName = request.RoundName,
                InterviewerId = request.InterviewerId,
                InterviewerName = $"{interviewer.FirstName} {interviewer.LastName}".Trim(),
                ScheduledAt = request.ScheduledAt,
                DurationMinutes = request.DurationMinutes > 0 ? request.DurationMinutes : 45,
                MeetingLink = request.MeetingLink,
                Status = "Scheduled",
                CreatedBy = (int)(empId > 0 ? empId : 1),
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.JobInterviews.Add(entity);
            _unitOfWork.Save();

            // Advance candidate stage and synchronize Candidate entity with interviewer assignment
            var cand = await _context.Candidates.FirstOrDefaultAsync(c => c.Id == request.CandidateId);
            if (cand != null)
            {
                if (cand.Stage == "Sourced" || cand.Stage == "Screening" || string.IsNullOrEmpty(cand.Stage))
                {
                    cand.Stage = "Interview";
                }

                var currentExpYears = "0";
                if (!string.IsNullOrWhiteSpace(cand.RelevantExperience))
                {
                    if (cand.RelevantExperience.Trim().StartsWith("{"))
                    {
                        try
                        {
                            var parsed = JsonSerializer.Deserialize<JsonElement>(cand.RelevantExperience);
                            if (parsed.TryGetProperty("years", out var yProp)) currentExpYears = yProp.GetString() ?? "0";
                        }
                        catch {}
                    }
                    else
                    {
                        currentExpYears = cand.RelevantExperience;
                    }
                }

                cand.RelevantExperience = JsonSerializer.Serialize(new
                {
                    years = currentExpYears,
                    interviewerId = entity.InterviewerId,
                    interviewerName = entity.InterviewerName,
                    currentRound = entity.RoundName,
                    scheduledAt = entity.ScheduledAt,
                    status = "Scheduled",
                    meetingLink = entity.MeetingLink
                });

                cand.UpdatedDate = DateTime.UtcNow;
                _context.Candidates.Update(cand);
                await _context.SaveChangesAsync();
            }

            return (await GetInterviews(candidateId: request.CandidateId)).First(i => i.Id == entity.Id);
        }

        public async Task<JobInterviewDto> SubmitInterviewFeedback(SubmitInterviewFeedbackRequest request)
        {
            var interview = await _unitOfWork.JobInterviews.GetById(request.InterviewId)
                ?? throw new KeyNotFoundException($"Interview with ID {request.InterviewId} not found");

            interview.Rating = request.Rating;
            interview.Recommendation = request.Recommendation;
            interview.FeedbackNotes = request.FeedbackNotes;
            interview.EvaluatedAt = DateTime.UtcNow;
            interview.Status = "Completed";
            interview.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.JobInterviews.Update(interview);
            _unitOfWork.Save();

            // Automatic stage progression & RelevantExperience sync
            var cand = await _context.Candidates.FirstOrDefaultAsync(c => c.Id == interview.CandidateId);
            if (cand != null)
            {
                var currentExpYears = "0";
                if (!string.IsNullOrWhiteSpace(cand.RelevantExperience))
                {
                    if (cand.RelevantExperience.Trim().StartsWith("{"))
                    {
                        try
                        {
                            var parsed = JsonSerializer.Deserialize<JsonElement>(cand.RelevantExperience);
                            if (parsed.TryGetProperty("years", out var yProp)) currentExpYears = yProp.GetString() ?? "0";
                        }
                        catch {}
                    }
                    else
                    {
                        currentExpYears = cand.RelevantExperience;
                    }
                }

                cand.RelevantExperience = JsonSerializer.Serialize(new
                {
                    years = currentExpYears,
                    interviewerId = interview.InterviewerId,
                    interviewerName = interview.InterviewerName,
                    remarks = interview.FeedbackNotes,
                    rating = interview.Rating,
                    recommendation = interview.Recommendation,
                    currentRound = interview.RoundName,
                    status = "Completed",
                    evaluationDate = DateTime.UtcNow.ToString("o")
                });

                if (request.Recommendation == "Reject")
                {
                    cand.Stage = "Rejected";
                }
                else if (request.AdvanceToNextStage && (request.Recommendation == "Strong Hire" || request.Recommendation == "Hire"))
                {
                    cand.Stage = "Offered";
                }
                else
                {
                    // Candidate remains in Interview stage to allow subsequent interview rounds
                    if (string.Equals(cand.Stage, "Sourced", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(cand.Stage, "Screening", StringComparison.OrdinalIgnoreCase) ||
                        string.IsNullOrWhiteSpace(cand.Stage))
                    {
                        cand.Stage = "Interview";
                    }
                }
                cand.UpdatedDate = DateTime.UtcNow;
                _context.Candidates.Update(cand);
                await _context.SaveChangesAsync();
            }

            return (await GetInterviews(candidateId: interview.CandidateId)).First(i => i.Id == interview.Id);
        }

        public async Task<bool> CancelInterview(long interviewId, string reason)
        {
            var interview = await _unitOfWork.JobInterviews.GetById(interviewId);
            if (interview == null) return false;

            interview.Status = "Cancelled";
            interview.FeedbackNotes = reason;
            interview.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.JobInterviews.Update(interview);
            _unitOfWork.Save();

            var cand = await _context.Candidates.FirstOrDefaultAsync(c => c.Id == interview.CandidateId);
            if (cand != null && !string.IsNullOrWhiteSpace(cand.RelevantExperience) && cand.RelevantExperience.Contains($"\"interviewerId\":{interview.InterviewerId}"))
            {
                cand.RelevantExperience = JsonSerializer.Serialize(new
                {
                    years = "0",
                    status = "Cancelled",
                    remarks = $"Round '{interview.RoundName}' was cancelled: {reason}"
                });
                cand.UpdatedDate = DateTime.UtcNow;
                _context.Candidates.Update(cand);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        // ==========================================
        // 4. OFFERS
        // ==========================================
        public async Task<List<JobOfferDto>> GetOffers(long? requisitionId = null, string? status = null)
        {
            var query = _context.JobOffers.Where(o => !o.IsDeleted);
            if (requisitionId.HasValue) query = query.Where(o => o.JobRequisitionId == requisitionId.Value);
            if (!string.IsNullOrWhiteSpace(status) && status != "All") query = query.Where(o => o.Status == status);

            var offers = await query.OrderByDescending(o => o.CreatedDate).ToListAsync();
            var candIds = offers.Select(o => o.CandidateId).Distinct().ToList();
            var candidates = await _context.Candidates.Where(c => candIds.Contains(c.Id)).ToListAsync();
            var reqs = await _context.JobRequisitions.Where(r => !r.IsDeleted).ToListAsync();

            return offers.Select(o =>
            {
                var cand = candidates.FirstOrDefault(c => c.Id == o.CandidateId);
                var req = reqs.FirstOrDefault(r => r.Id == o.JobRequisitionId);
                var isCandOnboarded = (cand != null && (string.Equals(cand.Stage, "Onboarded", StringComparison.OrdinalIgnoreCase) || string.Equals(cand.Stage, "Hired", StringComparison.OrdinalIgnoreCase))) || string.Equals(o.Status, "Onboarded", StringComparison.OrdinalIgnoreCase);
                return new JobOfferDto
                {
                    Id = o.Id,
                    CandidateId = o.CandidateId,
                    CandidateName = cand != null ? $"{cand.FirstName} {cand.LastName}".Trim() : string.Empty,
                    CandidateEmail = cand?.EmailAddress ?? string.Empty,
                    JobRequisitionId = o.JobRequisitionId,
                    JobTitle = req?.Title,
                    Designation = o.Designation,
                    OfferedCtc = o.OfferedCtc,
                    JoiningDate = o.JoiningDate,
                    ExpiryDate = o.ExpiryDate,
                    Status = isCandOnboarded ? "Onboarded" : o.Status,
                    Notes = o.Notes,
                    ApprovedBy = o.ApprovedBy,
                    IssuedDate = o.IssuedDate,
                    IsCandidateOnboarded = isCandOnboarded
                };
            }).ToList();
        }

        public async Task<JobOfferDto> CreateOffer(CreateJobOfferRequest request)
        {
            var empId = _userContext.GetCurrentEmployeeId();
            var issuer = await _context.Employees.FirstOrDefaultAsync(e => e.Id == empId);

            var entity = new JobOffer
            {
                CandidateId = request.CandidateId,
                JobRequisitionId = request.JobRequisitionId,
                Designation = request.Designation,
                OfferedCtc = request.OfferedCtc,
                JoiningDate = request.JoiningDate,
                ExpiryDate = request.ExpiryDate ?? request.JoiningDate.AddDays(7),
                Status = "Sent",
                Notes = request.Notes,
                ApprovedBy = issuer != null ? $"{issuer.FirstName} {issuer.LastName}".Trim() : "HR Department",
                IssuedDate = DateTime.UtcNow,
                CreatedBy = (int)(empId > 0 ? empId : 1),
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.JobOffers.Add(entity);
            _unitOfWork.Save();

            // Advance candidate stage to Offered
            var cand = await _context.Candidates.FirstOrDefaultAsync(c => c.Id == request.CandidateId);
            if (cand != null)
            {
                cand.Stage = "Offered";
                cand.UpdatedDate = DateTime.UtcNow;
                _context.Candidates.Update(cand);
                await _context.SaveChangesAsync();
            }

            return (await GetOffers(status: "All")).First(o => o.Id == entity.Id);
        }

        public async Task<JobOfferDto> UpdateOfferStatus(long offerId, string status, string? notes = null)
        {
            var offer = await _unitOfWork.JobOffers.GetById(offerId)
                ?? throw new KeyNotFoundException($"Job Offer with ID {offerId} not found");

            offer.Status = status;
            if (!string.IsNullOrEmpty(notes))
            {
                offer.Notes = string.IsNullOrEmpty(offer.Notes) ? notes : $"{offer.Notes}\n{notes}";
            }
            offer.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.JobOffers.Update(offer);
            _unitOfWork.Save();

            var cand = await _context.Candidates.FirstOrDefaultAsync(c => c.Id == offer.CandidateId);
            if (cand != null)
            {
                if (status == "Accepted")
                {
                    cand.Stage = "Hired";
                    // Increment filled positions on requisition
                    if (offer.JobRequisitionId.HasValue)
                    {
                        var req = await _unitOfWork.JobRequisitions.GetById(offer.JobRequisitionId.Value);
                        if (req != null)
                        {
                            req.FilledPositions += 1;
                            if (req.FilledPositions >= req.OpenPositions)
                            {
                                req.Status = "Closed";
                            }
                            _unitOfWork.JobRequisitions.Update(req);
                            _unitOfWork.Save();
                        }
                    }
                }
                else if (status == "Declined" || status == "Revoked")
                {
                    cand.Stage = "Rejected";
                }
                cand.UpdatedDate = DateTime.UtcNow;
                _context.Candidates.Update(cand);
                await _context.SaveChangesAsync();
            }

            return (await GetOffers(status: "All")).First(o => o.Id == offer.Id);
        }

        // ==========================================
        // 5. METRICS
        // ==========================================
        public async Task<RecruitmentDashboardMetricsDto> GetDashboardMetrics()
        {
            var openReqs = await _context.JobRequisitions.CountAsync(r => !r.IsDeleted && r.Status == "Open");
            var totalCands = await _context.Candidates.CountAsync(c => !c.IsDeleted);
            var activeInterviews = await _context.JobInterviews.CountAsync(i => !i.IsDeleted && i.Status == "Scheduled");
            var offersReleased = await _context.JobOffers.CountAsync(o => !o.IsDeleted);
            var offersAccepted = await _context.JobOffers.CountAsync(o => !o.IsDeleted && o.Status == "Accepted");
            var hiredCount = await _context.Candidates.CountAsync(c => !c.IsDeleted && (c.Stage == "Hired" || c.Stage == "Onboarded"));

            return new RecruitmentDashboardMetricsDto
            {
                OpenRequisitionsCount = openReqs,
                TotalCandidatesInPipeline = totalCands,
                ActiveInterviewsScheduled = activeInterviews,
                OffersReleasedCount = offersReleased,
                OffersAcceptedCount = offersAccepted,
                HiredThisQuarterCount = hiredCount
            };
        }
    }
}
