using KHRMS.Core;

namespace KHRMS.Services
{
    public class CandidateService(IUnitOfWork unitOfWork) : ICandidateService
    {
        public IUnitOfWork _unitOfWork = unitOfWork;
        public async Task<bool> CreateCandidate(Candidate candidate)
        {
            if (candidate != null)
            {
                candidate.CreatedDate = DateTime.Now;
                await _unitOfWork.Candidates.Add(candidate);

                var result = _unitOfWork.Save();

                if (result > 0)
                    return true;
                else
                    return false;
            }
            return false;
        }
        public async Task<bool> DeleteCandidate(long candidateId)
        {
            if (candidateId > 0)
            {
                var candidateDetails = await _unitOfWork.Candidates.GetById(candidateId);
                if (candidateDetails != null)
                {
                    candidateDetails.IsDeleted = true;
                    candidateDetails.IsActive = false;
                    
                    _unitOfWork.Candidates.Update(candidateDetails);
                    var result = _unitOfWork.Save();
                    if (result > 0)
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }
        public async Task<IEnumerable<Candidate>> GetAllCandidates()
        {
            var candidates = await _unitOfWork.Candidates.GetAll();
            return candidates;
        }
        public async Task<Candidate> GetCandidateById(int candidateId)
        {
            if (candidateId > 0)
            {
                var candidateDetails = await _unitOfWork.Candidates.GetById(candidateId);
                if (candidateDetails != null)
                {
                    return candidateDetails;
                }
            }
            return null;
        }
        public async Task<bool> UpdateCandidate(Candidate candidate)
        {
            if (candidate != null)
            {
                var candidateDetails = await _unitOfWork.Candidates.GetById(candidate.Id);
                if (candidateDetails != null)
                {
                    candidateDetails.FirstName = candidate.FirstName;
                    candidateDetails.LastName = candidate.LastName;
                    candidateDetails.EmailAddress = candidate.EmailAddress;
                    candidateDetails.MobileNumber = candidate.MobileNumber;
                    candidateDetails.CurrentSalary = candidate.CurrentSalary;
                    candidateDetails.ExpectedSalary = candidate.ExpectedSalary;
                    candidateDetails.TotalExperience = candidate.TotalExperience;
                    candidateDetails.RelevantExperience = candidate.RelevantExperience;
                    candidateDetails.NoticePeriod = candidate.NoticePeriod;
                    candidateDetails.UpdatedDate = DateTime.Now;
                    candidateDetails.AppliedRole = candidate.AppliedRole;
                    candidateDetails.Stage = candidate.Stage;
                    candidateDetails.MatchScore = candidate.MatchScore;
                    // ✅ Ensure IsActive status is updated
                    candidateDetails.IsActive = candidate.IsActive;
                    _unitOfWork.Candidates.Update(candidateDetails);
                    var result = _unitOfWork.Save();
                    if (result > 0)
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }



    }
}
