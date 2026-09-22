using KHRMS.Core;

namespace KHRMS.Services
{
    public interface ICandidateService
    {
        Task<bool> CreateCandidate(Candidate candidate);
        Task<IEnumerable<Candidate>> GetAllCandidates();
        Task<Candidate> GetCandidateById(int candidateId);
        Task<bool> UpdateCandidate(Candidate candidate);
        Task<bool> DeleteCandidate(long candidateId);
        Task<Request.CandidateOnboardResponse> OnboardCandidate(Request.CandidateOnboardRequest request);
    }
}
