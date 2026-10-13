using KHRMS.Core.Models;
using KHRMS.Services.Request;

namespace KHRMS.Services.Interfaces
{
    public interface IPmsService
    {
        // 1. Goals & OKRs
        Task<List<PmsGoalDto>> GetGoalsAsync(long? employeeId = null, string? period = null, string? category = null);
        Task<PmsGoalDto?> GetGoalByIdAsync(long id);
        Task<PmsGoalDto> CreateGoalAsync(CreatePmsGoalRequest request);
        Task<PmsGoalDto> UpdateGoalAsync(UpdatePmsGoalRequest request);
        Task<bool> DeleteGoalAsync(long id);
        Task<PmsGoalDto> RecordCheckInAsync(PmsGoalCheckInRequest request);

        // 2. Review Cycles & Appraisals
        Task<List<PmsReviewCycle>> GetReviewCyclesAsync();
        Task<PmsReviewCycle> CreateReviewCycleAsync(CreateReviewCycleRequest request);
        Task<List<PmsAppraisalDto>> GetAppraisalsAsync(long? cycleId = null, long? employeeId = null, long? managerId = null);
        Task<PmsAppraisalDto?> GetAppraisalByIdAsync(long id);
        Task<PmsAppraisalDto> SubmitSelfReviewAsync(SubmitSelfReviewRequest request);
        Task<PmsAppraisalDto> SubmitManagerReviewAsync(SubmitManagerReviewRequest request);
        Task<PmsAppraisalDto> FinalizeAppraisalAsync(FinalizeAppraisalRequest request);

        // 3. Continuous Feedback
        Task<List<PmsFeedbackDto>> GetFeedbackAsync(long? employeeId = null, string? feedbackType = null);
        Task<PmsFeedbackDto> GiveFeedbackAsync(PmsFeedbackRequest request);

        // 4. 1-on-1s
        Task<List<PmsOneOnOneDto>> GetOneOnOnesAsync(long? managerId = null, long? employeeId = null);
        Task<PmsOneOnOneDto> SaveOneOnOneAsync(PmsOneOnOneRequest request);

        // 5. Performance Improvement Plans (PIP)
        Task<List<PmsPipDto>> GetPipsAsync(long? employeeId = null);
        Task<PmsPipDto> SavePipAsync(PmsPipRequest request);
    }
}
