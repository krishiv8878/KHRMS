using System.Collections.Generic;
using System.Threading.Tasks;
using KHRMS.Services.Request;

namespace KHRMS.Services.Interfaces
{
    public interface IRecruitmentService
    {
        // Requisitions
        Task<List<JobRequisitionDto>> GetAllRequisitions(string? status = null);
        Task<JobRequisitionDto?> GetRequisitionById(long id);
        Task<JobRequisitionDto> CreateRequisition(CreateJobRequisitionRequest request);
        Task<JobRequisitionDto> UpdateRequisition(long id, CreateJobRequisitionRequest request);
        Task<bool> UpdateRequisitionStatus(long id, string status);
        Task<bool> DeleteRequisition(long id);

        // Candidates Pipeline
        Task<List<CandidatePipelineItemDto>> GetPipelineCandidates(long? requisitionId = null, string? stage = null);
        Task<CandidatePipelineItemDto?> GetCandidatePipelineDetail(long candidateId);
        Task<bool> UpdateCandidateStage(UpdateCandidateStageRequest request);

        // Interviews
        Task<List<JobInterviewDto>> GetInterviews(long? candidateId = null, long? interviewerId = null, string? status = null);
        Task<JobInterviewDto> ScheduleInterview(ScheduleInterviewRequest request);
        Task<JobInterviewDto> SubmitInterviewFeedback(SubmitInterviewFeedbackRequest request);
        Task<bool> CancelInterview(long interviewId, string reason);

        // Offers
        Task<List<JobOfferDto>> GetOffers(long? requisitionId = null, string? status = null);
        Task<JobOfferDto> CreateOffer(CreateJobOfferRequest request);
        Task<JobOfferDto> UpdateOfferStatus(long offerId, string status, string? notes = null);

        // Metrics
        Task<RecruitmentDashboardMetricsDto> GetDashboardMetrics();
    }
}
