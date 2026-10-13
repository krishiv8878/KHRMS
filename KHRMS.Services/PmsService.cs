using System.Text.Json;
using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.EntityFrameworkCore;

namespace KHRMS.Services
{
    public class PmsService : IPmsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContext;
        private readonly KHRMSContextClass _dbContext;
        private static bool _tablesInitialized = false;

        public PmsService(IUnitOfWork unitOfWork, IUserContextService userContext, KHRMSContextClass dbContext)
        {
            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _dbContext = dbContext;
        }


        // ================= 1. GOALS & OKRS =================
        public async Task<List<PmsGoalDto>> GetGoalsAsync(long? employeeId = null, string? period = null, string? category = null)
        {
            

            var goals = (await _unitOfWork.PmsGoals.GetAll()).ToList();
            if (employeeId.HasValue && employeeId.Value > 0)
            {
                goals = goals.Where(g => g.EmployeeId == employeeId.Value).ToList();
            }
            if (!string.IsNullOrEmpty(period))
            {
                goals = goals.Where(g => g.Period.Equals(period, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (!string.IsNullOrEmpty(category))
            {
                goals = goals.Where(g => g.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var allKeyResults = (await _unitOfWork.PmsKeyResults.GetAll()).ToList();
            var allCheckIns = (await _unitOfWork.PmsGoalCheckIns.GetAll()).ToList();
            var allEmployees = (await _unitOfWork.Employees.GetAll()).ToList();

            var result = new List<PmsGoalDto>();
            foreach (var g in goals)
            {
                var emp = allEmployees.FirstOrDefault(e => e.Id == g.EmployeeId);
                result.Add(new PmsGoalDto
                {
                    Goal = g,
                    KeyResults = allKeyResults.Where(k => k.GoalId == g.Id).ToList(),
                    CheckIns = allCheckIns.Where(c => c.GoalId == g.Id).OrderByDescending(c => c.CheckInDate).Take(10).ToList(),
                    EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}".Trim() : $"EMP-{g.EmployeeId:D4}",
                    EmployeeCode = emp?.EmployeeCode?.ToString() ?? $"EMP-{g.EmployeeId:D4}",
                    Designation = emp?.Designation ?? "Personnel"
                });
            }

            return result;
        }

        public async Task<PmsGoalDto?> GetGoalByIdAsync(long id)
        {
            
            var g = await _unitOfWork.PmsGoals.GetById(id);
            if (g == null) return null;

            var keyResults = (await _unitOfWork.PmsKeyResults.GetAll()).Where(k => k.GoalId == id).ToList();
            var checkIns = (await _unitOfWork.PmsGoalCheckIns.GetAll()).Where(c => c.GoalId == id).OrderByDescending(c => c.CheckInDate).ToList();
            var emp = await _unitOfWork.Employees.GetById(g.EmployeeId);

            return new PmsGoalDto
            {
                Goal = g,
                KeyResults = keyResults,
                CheckIns = checkIns,
                EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}".Trim() : $"EMP-{g.EmployeeId:D4}",
                EmployeeCode = emp?.EmployeeCode?.ToString() ?? $"EMP-{g.EmployeeId:D4}",
                Designation = emp?.Designation ?? "Personnel"
            };
        }

        public async Task<PmsGoalDto> CreateGoalAsync(CreatePmsGoalRequest request)
        {
            
            long currentUserId = _userContext.GetCurrentEmployeeId();
            long targetEmpId = request.EmployeeId > 0 ? request.EmployeeId : currentUserId;

            var goal = new PmsGoal
            {
                Title = request.Title,
                Description = request.Description,
                EmployeeId = targetEmpId,
                Category = request.Category,
                Department = request.Department,
                Period = request.Period,
                Weightage = request.Weightage,
                ProgressPercentage = 0,
                Status = "Not Started",
                DueDate = request.DueDate,
                CreatedBy = (int)currentUserId,
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = (int)currentUserId,
                UpdatedDate = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.PmsGoals.Add(goal);
            _unitOfWork.Save();

            var createdKrs = new List<PmsKeyResult>();
            if (request.KeyResults != null && request.KeyResults.Any())
            {
                foreach (var kr in request.KeyResults)
                {
                    var newKr = new PmsKeyResult
                    {
                        GoalId = goal.Id,
                        Title = kr.Title,
                        TargetValue = kr.TargetValue,
                        CurrentValue = kr.CurrentValue,
                        MetricUnit = kr.MetricUnit,
                        Weightage = kr.Weightage,
                        CreatedBy = (int)currentUserId,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedBy = (int)currentUserId,
                        UpdatedDate = DateTime.UtcNow,
                        IsActive = true
                    };
                    await _unitOfWork.PmsKeyResults.Add(newKr);
                    createdKrs.Add(newKr);
                }
                _unitOfWork.Save();
            }

            RecalculateGoalProgress(goal, createdKrs);
            _unitOfWork.PmsGoals.Update(goal);
            _unitOfWork.Save();

            return (await GetGoalByIdAsync(goal.Id))!;
        }

        public async Task<PmsGoalDto> UpdateGoalAsync(UpdatePmsGoalRequest request)
        {
            
            var goal = await _unitOfWork.PmsGoals.GetById(request.Id);
            if (goal == null) throw new KeyNotFoundException($"Goal {request.Id} not found");

            long currentUserId = _userContext.GetCurrentEmployeeId();
            goal.Title = request.Title;
            goal.Description = request.Description;
            goal.Category = request.Category;
            goal.Department = request.Department;
            goal.Period = request.Period;
            goal.Weightage = request.Weightage;
            goal.Status = request.Status;
            goal.DueDate = request.DueDate;
            goal.UpdatedBy = (int)currentUserId;
            goal.UpdatedDate = DateTime.UtcNow;

            var existingKrs = (await _unitOfWork.PmsKeyResults.GetAll()).Where(k => k.GoalId == goal.Id).ToList();
            if (request.KeyResults != null)
            {
                foreach (var reqKr in request.KeyResults)
                {
                    if (reqKr.Id > 0)
                    {
                        var match = existingKrs.FirstOrDefault(k => k.Id == reqKr.Id);
                        if (match != null)
                        {
                            match.Title = reqKr.Title;
                            match.TargetValue = reqKr.TargetValue;
                            match.CurrentValue = reqKr.CurrentValue;
                            match.MetricUnit = reqKr.MetricUnit;
                            match.Weightage = reqKr.Weightage;
                            match.UpdatedDate = DateTime.UtcNow;
                            _unitOfWork.PmsKeyResults.Update(match);
                        }
                    }
                    else
                    {
                        var newKr = new PmsKeyResult
                        {
                            GoalId = goal.Id,
                            Title = reqKr.Title,
                            TargetValue = reqKr.TargetValue,
                            CurrentValue = reqKr.CurrentValue,
                            MetricUnit = reqKr.MetricUnit,
                            Weightage = reqKr.Weightage,
                            CreatedBy = (int)currentUserId,
                            CreatedDate = DateTime.UtcNow,
                            UpdatedDate = DateTime.UtcNow,
                            IsActive = true
                        };
                        await _unitOfWork.PmsKeyResults.Add(newKr);
                        existingKrs.Add(newKr);
                    }
                }
            }

            RecalculateGoalProgress(goal, existingKrs);
            _unitOfWork.PmsGoals.Update(goal);
            _unitOfWork.Save();

            return (await GetGoalByIdAsync(goal.Id))!;
        }

        public async Task<bool> DeleteGoalAsync(long id)
        {
            
            var goal = await _unitOfWork.PmsGoals.GetById(id);
            if (goal == null) return false;

            goal.IsDeleted = true;
            goal.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.PmsGoals.Update(goal);

            var krs = (await _unitOfWork.PmsKeyResults.GetAll()).Where(k => k.GoalId == id).ToList();
            foreach (var k in krs)
            {
                k.IsDeleted = true;
                _unitOfWork.PmsKeyResults.Update(k);
            }

            _unitOfWork.Save();
            return true;
        }

        public async Task<PmsGoalDto> RecordCheckInAsync(PmsGoalCheckInRequest request)
        {
            
            var goal = await _unitOfWork.PmsGoals.GetById(request.GoalId);
            if (goal == null) throw new KeyNotFoundException($"Goal {request.GoalId} not found");

            long currentUserId = _userContext.GetCurrentEmployeeId();

            decimal prevVal = 0;
            if (request.KeyResultId.HasValue && request.KeyResultId.Value > 0)
            {
                var kr = await _unitOfWork.PmsKeyResults.GetById(request.KeyResultId.Value);
                if (kr != null)
                {
                    prevVal = kr.CurrentValue;
                    kr.CurrentValue = request.NewValue;
                    kr.UpdatedDate = DateTime.UtcNow;
                    _unitOfWork.PmsKeyResults.Update(kr);
                }
            }
            else
            {
                prevVal = goal.ProgressPercentage;
                goal.ProgressPercentage = (int)Math.Clamp(request.NewValue, 0, 100);
            }

            var checkIn = new PmsGoalCheckIn
            {
                GoalId = goal.Id,
                KeyResultId = request.KeyResultId,
                PreviousValue = prevVal,
                NewValue = request.NewValue,
                ConfidenceScore = request.ConfidenceScore,
                Notes = request.Notes,
                CheckInDate = DateTime.UtcNow,
                EmployeeId = currentUserId,
                CreatedBy = (int)currentUserId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.PmsGoalCheckIns.Add(checkIn);

            var allKrs = (await _unitOfWork.PmsKeyResults.GetAll()).Where(k => k.GoalId == goal.Id).ToList();
            RecalculateGoalProgress(goal, allKrs);

            _unitOfWork.PmsGoals.Update(goal);
            _unitOfWork.Save();

            return (await GetGoalByIdAsync(goal.Id))!;
        }

        private void RecalculateGoalProgress(PmsGoal goal, List<PmsKeyResult> keyResults)
        {
            if (keyResults != null && keyResults.Any())
            {
                decimal totalWeight = keyResults.Sum(k => k.Weightage > 0 ? k.Weightage : 1);
                decimal weightedProgress = 0;
                foreach (var k in keyResults)
                {
                    decimal weight = k.Weightage > 0 ? k.Weightage : 1;
                    decimal ratio = k.TargetValue > 0 ? Math.Clamp(k.CurrentValue / k.TargetValue, 0, 1) : 0;
                    weightedProgress += (ratio * weight);
                }
                goal.ProgressPercentage = (int)Math.Round((weightedProgress / totalWeight) * 100);
            }

            if (goal.ProgressPercentage >= 100)
            {
                goal.Status = "Completed";
            }
            else if (goal.ProgressPercentage >= 60)
            {
                goal.Status = "On Track";
            }
            else if (goal.ProgressPercentage >= 30)
            {
                goal.Status = "At Risk";
            }
            else if (goal.ProgressPercentage > 0)
            {
                goal.Status = "Behind";
            }
        }

        // ================= 2. REVIEW CYCLES & APPRAISALS =================
        public async Task<List<PmsReviewCycle>> GetReviewCyclesAsync()
        {
            
            var cycles = (await _unitOfWork.PmsReviewCycles.GetAll()).OrderByDescending(c => c.StartDate).ToList();
            if (!cycles.Any())
            {
                // Create standard default cycle
                var defaultCycle = new PmsReviewCycle
                {
                    CycleName = "Annual Performance Appraisal 2026",
                    Description = "Comprehensive annual review encompassing goal evaluation, core competencies, and self-manager assessment.",
                    Period = "Annual",
                    StartDate = new DateTime(DateTime.UtcNow.Year, 1, 1),
                    EndDate = new DateTime(DateTime.UtcNow.Year, 12, 31),
                    SelfReviewDeadline = DateTime.UtcNow.AddDays(14),
                    ManagerReviewDeadline = DateTime.UtcNow.AddDays(28),
                    Status = "Active",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    IsActive = true
                };
                await _unitOfWork.PmsReviewCycles.Add(defaultCycle);
                _unitOfWork.Save();
                cycles.Add(defaultCycle);
            }
            return cycles;
        }

        public async Task<PmsReviewCycle> CreateReviewCycleAsync(CreateReviewCycleRequest request)
        {
            
            long currentUserId = _userContext.GetCurrentEmployeeId();

            var cycle = new PmsReviewCycle
            {
                CycleName = request.CycleName,
                Description = request.Description,
                Period = request.Period,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                SelfReviewDeadline = request.SelfReviewDeadline,
                ManagerReviewDeadline = request.ManagerReviewDeadline,
                Status = "Active",
                CreatedBy = (int)currentUserId,
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = (int)currentUserId,
                UpdatedDate = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.PmsReviewCycles.Add(cycle);
            _unitOfWork.Save();

            // Auto-initialize Appraisal forms for all active employees
            var employees = (await _unitOfWork.Employees.GetAll()).Where(e => e.IsActive && !e.IsDeleted).ToList();
            foreach (var emp in employees)
            {
                var appraisal = new PmsAppraisal
                {
                    ReviewCycleId = cycle.Id,
                    EmployeeId = emp.Id,
                    ManagerId = emp.CreatedBy > 0 ? emp.CreatedBy : null,
                    Status = "Self Review Pending",
                    CreatedBy = (int)currentUserId,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    IsActive = true
                };
                await _unitOfWork.PmsAppraisals.Add(appraisal);
            }
            _unitOfWork.Save();

            return cycle;
        }

        public async Task<List<PmsAppraisalDto>> GetAppraisalsAsync(long? cycleId = null, long? employeeId = null, long? managerId = null)
        {
            
            var appraisals = (await _unitOfWork.PmsAppraisals.GetAll()).ToList();
            var cycles = (await _unitOfWork.PmsReviewCycles.GetAll()).ToList();
            var employees = (await _unitOfWork.Employees.GetAll()).ToList();

            if (cycleId.HasValue && cycleId.Value > 0)
            {
                appraisals = appraisals.Where(a => a.ReviewCycleId == cycleId.Value).ToList();
            }
            if (employeeId.HasValue && employeeId.Value > 0)
            {
                appraisals = appraisals.Where(a => a.EmployeeId == employeeId.Value).ToList();
            }
            if (managerId.HasValue && managerId.Value > 0)
            {
                appraisals = appraisals.Where(a => a.ManagerId == managerId.Value).ToList();
            }

            // If empty, auto-create appraisal for employee in active cycle
            if (!appraisals.Any() && employeeId.HasValue && employeeId.Value > 0)
            {
                var activeCycle = cycles.FirstOrDefault(c => c.Status == "Active") ?? cycles.FirstOrDefault();
                if (activeCycle != null)
                {
                    var newApp = new PmsAppraisal
                    {
                        ReviewCycleId = activeCycle.Id,
                        EmployeeId = employeeId.Value,
                        Status = "Self Review Pending",
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow,
                        IsActive = true
                    };
                    await _unitOfWork.PmsAppraisals.Add(newApp);
                    _unitOfWork.Save();
                    appraisals.Add(newApp);
                }
            }

            var result = new List<PmsAppraisalDto>();
            foreach (var a in appraisals)
            {
                var cycle = cycles.FirstOrDefault(c => c.Id == a.ReviewCycleId);
                var emp = employees.FirstOrDefault(e => e.Id == a.EmployeeId);
                var mgr = a.ManagerId.HasValue ? employees.FirstOrDefault(e => e.Id == a.ManagerId.Value) : null;

                result.Add(new PmsAppraisalDto
                {
                    Appraisal = a,
                    Cycle = cycle,
                    EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}".Trim() : $"EMP-{a.EmployeeId:D4}",
                    EmployeeCode = emp?.EmployeeCode?.ToString() ?? $"EMP-{a.EmployeeId:D4}",
                    Department = emp?.Branch ?? "General",
                    Designation = emp?.Designation ?? "Staff",
                    ManagerName = mgr != null ? $"{mgr.FirstName} {mgr.LastName}".Trim() : "Lead Reviewer"
                });
            }

            return result;
        }

        public async Task<PmsAppraisalDto?> GetAppraisalByIdAsync(long id)
        {
            
            var a = await _unitOfWork.PmsAppraisals.GetById(id);
            if (a == null) return null;

            var cycle = await _unitOfWork.PmsReviewCycles.GetById(a.ReviewCycleId);
            var emp = await _unitOfWork.Employees.GetById(a.EmployeeId);
            var mgr = a.ManagerId.HasValue ? await _unitOfWork.Employees.GetById(a.ManagerId.Value) : null;

            return new PmsAppraisalDto
            {
                Appraisal = a,
                Cycle = cycle,
                EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}".Trim() : $"EMP-{a.EmployeeId:D4}",
                EmployeeCode = emp?.EmployeeCode?.ToString() ?? $"EMP-{a.EmployeeId:D4}",
                Department = emp?.Branch ?? "General",
                Designation = emp?.Designation ?? "Staff",
                ManagerName = mgr != null ? $"{mgr.FirstName} {mgr.LastName}".Trim() : "Lead Reviewer"
            };
        }

        public async Task<PmsAppraisalDto> SubmitSelfReviewAsync(SubmitSelfReviewRequest request)
        {
            
            var a = await _unitOfWork.PmsAppraisals.GetById(request.AppraisalId);
            if (a == null) throw new KeyNotFoundException($"Appraisal {request.AppraisalId} not found");

            a.SelfRating = request.SelfRating;
            a.SelfComments = request.SelfComments;
            a.SelfSubmittedDate = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(request.CompetencyRatingsJson))
            {
                a.CompetencyRatingsJson = request.CompetencyRatingsJson;
            }
            a.Status = "Manager Review Pending";
            a.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.PmsAppraisals.Update(a);
            _unitOfWork.Save();

            return (await GetAppraisalByIdAsync(a.Id))!;
        }

        public async Task<PmsAppraisalDto> SubmitManagerReviewAsync(SubmitManagerReviewRequest request)
        {
            
            var a = await _unitOfWork.PmsAppraisals.GetById(request.AppraisalId);
            if (a == null) throw new KeyNotFoundException($"Appraisal {request.AppraisalId} not found");

            long currentUserId = _userContext.GetCurrentEmployeeId();
            a.ManagerId = currentUserId;
            a.ManagerRating = request.ManagerRating;
            a.ManagerComments = request.ManagerComments;
            a.ManagerSubmittedDate = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(request.CompetencyRatingsJson))
            {
                a.CompetencyRatingsJson = request.CompetencyRatingsJson;
            }
            a.FinalRating = request.ManagerRating;
            a.Status = "HR Review Pending";
            a.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.PmsAppraisals.Update(a);
            _unitOfWork.Save();

            return (await GetAppraisalByIdAsync(a.Id))!;
        }

        public async Task<PmsAppraisalDto> FinalizeAppraisalAsync(FinalizeAppraisalRequest request)
        {
            
            var a = await _unitOfWork.PmsAppraisals.GetById(request.AppraisalId);
            if (a == null) throw new KeyNotFoundException($"Appraisal {request.AppraisalId} not found");

            a.FinalRating = request.FinalRating;
            a.HrComments = request.HrComments;
            a.Status = "Completed";
            a.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.PmsAppraisals.Update(a);
            _unitOfWork.Save();

            return (await GetAppraisalByIdAsync(a.Id))!;
        }

        // ================= 3. CONTINUOUS FEEDBACK & PRAISE =================
        public async Task<List<PmsFeedbackDto>> GetFeedbackAsync(long? employeeId = null, string? feedbackType = null)
        {
            
            var feedbacks = (await _unitOfWork.PmsFeedbacks.GetAll()).OrderByDescending(f => f.CreatedDate).ToList();
            if (employeeId.HasValue && employeeId.Value > 0)
            {
                feedbacks = feedbacks.Where(f => f.ToEmployeeId == employeeId.Value || f.FromEmployeeId == employeeId.Value).ToList();
            }
            if (!string.IsNullOrEmpty(feedbackType))
            {
                feedbacks = feedbacks.Where(f => f.FeedbackType.Equals(feedbackType, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var employees = (await _unitOfWork.Employees.GetAll()).ToList();
            var result = new List<PmsFeedbackDto>();
            foreach (var f in feedbacks)
            {
                var sender = employees.FirstOrDefault(e => e.Id == f.FromEmployeeId);
                var receiver = employees.FirstOrDefault(e => e.Id == f.ToEmployeeId);

                result.Add(new PmsFeedbackDto
                {
                    Feedback = f,
                    FromEmployeeName = sender != null ? $"{sender.FirstName} {sender.LastName}".Trim() : $"EMP-{f.FromEmployeeId:D4}",
                    FromEmployeeDesignation = sender?.Designation ?? "Colleague",
                    ToEmployeeName = receiver != null ? $"{receiver.FirstName} {receiver.LastName}".Trim() : $"EMP-{f.ToEmployeeId:D4}",
                    ToEmployeeDesignation = receiver?.Designation ?? "Colleague"
                });
            }

            return result;
        }

        public async Task<PmsFeedbackDto> GiveFeedbackAsync(PmsFeedbackRequest request)
        {
            
            long currentUserId = _userContext.GetCurrentEmployeeId();

            var feedback = new PmsFeedback
            {
                FromEmployeeId = currentUserId,
                ToEmployeeId = request.ToEmployeeId,
                FeedbackType = request.FeedbackType,
                Badge = request.Badge,
                Message = request.Message,
                IsPrivate = request.IsPrivate,
                ReviewCycleId = request.ReviewCycleId,
                CreatedBy = (int)currentUserId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.PmsFeedbacks.Add(feedback);
            _unitOfWork.Save();

            var sender = await _unitOfWork.Employees.GetById(currentUserId);
            var receiver = await _unitOfWork.Employees.GetById(request.ToEmployeeId);

            return new PmsFeedbackDto
            {
                Feedback = feedback,
                FromEmployeeName = sender != null ? $"{sender.FirstName} {sender.LastName}".Trim() : $"EMP-{currentUserId:D4}",
                FromEmployeeDesignation = sender?.Designation ?? "Colleague",
                ToEmployeeName = receiver != null ? $"{receiver.FirstName} {receiver.LastName}".Trim() : $"EMP-{request.ToEmployeeId:D4}",
                ToEmployeeDesignation = receiver?.Designation ?? "Colleague"
            };
        }

        // ================= 4. 1-ON-1 MEETINGS =================
        public async Task<List<PmsOneOnOneDto>> GetOneOnOnesAsync(long? managerId = null, long? employeeId = null)
        {
            
            var meetings = (await _unitOfWork.PmsOneOnOnes.GetAll()).OrderByDescending(m => m.ScheduledDate).ToList();
            if (managerId.HasValue && managerId.Value > 0)
            {
                meetings = meetings.Where(m => m.ManagerId == managerId.Value).ToList();
            }
            if (employeeId.HasValue && employeeId.Value > 0)
            {
                meetings = meetings.Where(m => m.EmployeeId == employeeId.Value).ToList();
            }

            var employees = (await _unitOfWork.Employees.GetAll()).ToList();
            var result = new List<PmsOneOnOneDto>();
            foreach (var m in meetings)
            {
                var mgr = employees.FirstOrDefault(e => e.Id == m.ManagerId);
                var emp = employees.FirstOrDefault(e => e.Id == m.EmployeeId);

                result.Add(new PmsOneOnOneDto
                {
                    OneOnOne = m,
                    ManagerName = mgr != null ? $"{mgr.FirstName} {mgr.LastName}".Trim() : "Manager",
                    EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}".Trim() : $"EMP-{m.EmployeeId:D4}",
                    EmployeeDesignation = emp?.Designation ?? "Team Member"
                });
            }

            return result;
        }

        public async Task<PmsOneOnOneDto> SaveOneOnOneAsync(PmsOneOnOneRequest request)
        {
            
            long currentUserId = _userContext.GetCurrentEmployeeId();

            PmsOneOnOne meeting;
            if (request.Id.HasValue && request.Id.Value > 0)
            {
                meeting = await _unitOfWork.PmsOneOnOnes.GetById(request.Id.Value) ?? throw new KeyNotFoundException($"Meeting {request.Id} not found");
                meeting.ScheduledDate = request.ScheduledDate;
                meeting.Agenda = request.Agenda;
                meeting.TalkingPointsJson = request.TalkingPointsJson;
                meeting.ActionItemsJson = request.ActionItemsJson;
                meeting.Notes = request.Notes;
                meeting.Status = request.Status;
                meeting.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.PmsOneOnOnes.Update(meeting);
            }
            else
            {
                meeting = new PmsOneOnOne
                {
                    ManagerId = currentUserId,
                    EmployeeId = request.EmployeeId,
                    ScheduledDate = request.ScheduledDate,
                    Agenda = request.Agenda,
                    TalkingPointsJson = request.TalkingPointsJson,
                    ActionItemsJson = request.ActionItemsJson,
                    Notes = request.Notes,
                    Status = request.Status,
                    CreatedBy = (int)currentUserId,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    IsActive = true
                };
                await _unitOfWork.PmsOneOnOnes.Add(meeting);
            }

            _unitOfWork.Save();

            var mgr = await _unitOfWork.Employees.GetById(meeting.ManagerId);
            var emp = await _unitOfWork.Employees.GetById(meeting.EmployeeId);

            return new PmsOneOnOneDto
            {
                OneOnOne = meeting,
                ManagerName = mgr != null ? $"{mgr.FirstName} {mgr.LastName}".Trim() : "Manager",
                EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}".Trim() : $"EMP-{meeting.EmployeeId:D4}",
                EmployeeDesignation = emp?.Designation ?? "Team Member"
            };
        }

        // ================= 5. PERFORMANCE IMPROVEMENT PLANS (PIP) =================
        public async Task<List<PmsPipDto>> GetPipsAsync(long? employeeId = null)
        {
            
            var pips = (await _unitOfWork.PmsPips.GetAll()).OrderByDescending(p => p.StartDate).ToList();
            if (employeeId.HasValue && employeeId.Value > 0)
            {
                pips = pips.Where(p => p.EmployeeId == employeeId.Value).ToList();
            }

            var employees = (await _unitOfWork.Employees.GetAll()).ToList();
            var result = new List<PmsPipDto>();
            foreach (var p in pips)
            {
                var emp = employees.FirstOrDefault(e => e.Id == p.EmployeeId);
                var mgr = employees.FirstOrDefault(e => e.Id == p.ManagerId);

                result.Add(new PmsPipDto
                {
                    Pip = p,
                    EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}".Trim() : $"EMP-{p.EmployeeId:D4}",
                    EmployeeCode = emp?.EmployeeCode?.ToString() ?? $"EMP-{p.EmployeeId:D4}",
                    Department = emp?.Branch ?? "General",
                    ManagerName = mgr != null ? $"{mgr.FirstName} {mgr.LastName}".Trim() : "Supervisor"
                });
            }

            return result;
        }

        public async Task<PmsPipDto> SavePipAsync(PmsPipRequest request)
        {
            
            long currentUserId = _userContext.GetCurrentEmployeeId();

            PmsPip pip;
            if (request.Id.HasValue && request.Id.Value > 0)
            {
                pip = await _unitOfWork.PmsPips.GetById(request.Id.Value) ?? throw new KeyNotFoundException($"PIP {request.Id} not found");
                pip.Reason = request.Reason;
                pip.DurationDays = request.DurationDays;
                pip.StartDate = request.StartDate;
                pip.EndDate = request.StartDate.AddDays(request.DurationDays);
                pip.ObjectivesJson = request.ObjectivesJson;
                pip.CheckInFrequency = request.CheckInFrequency;
                pip.Outcome = request.Outcome;
                pip.FinalComments = request.FinalComments;
                pip.Status = request.Status;
                pip.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.PmsPips.Update(pip);
            }
            else
            {
                pip = new PmsPip
                {
                    EmployeeId = request.EmployeeId,
                    ManagerId = currentUserId,
                    Reason = request.Reason,
                    DurationDays = request.DurationDays,
                    StartDate = request.StartDate,
                    EndDate = request.StartDate.AddDays(request.DurationDays),
                    ObjectivesJson = request.ObjectivesJson,
                    CheckInFrequency = request.CheckInFrequency,
                    Outcome = request.Outcome,
                    FinalComments = request.FinalComments,
                    Status = request.Status,
                    CreatedBy = (int)currentUserId,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    IsActive = true
                };
                await _unitOfWork.PmsPips.Add(pip);
            }

            _unitOfWork.Save();

            var emp = await _unitOfWork.Employees.GetById(pip.EmployeeId);
            var mgr = await _unitOfWork.Employees.GetById(pip.ManagerId);

            return new PmsPipDto
            {
                Pip = pip,
                EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}".Trim() : $"EMP-{pip.EmployeeId:D4}",
                EmployeeCode = emp?.EmployeeCode?.ToString() ?? $"EMP-{pip.EmployeeId:D4}",
                Department = emp?.Branch ?? "General",
                ManagerName = mgr != null ? $"{mgr.FirstName} {mgr.LastName}".Trim() : "Supervisor"
            };
        }
    }
}
