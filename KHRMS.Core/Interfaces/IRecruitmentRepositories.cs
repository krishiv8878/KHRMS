using KHRMS.Core.Models;

namespace KHRMS.Core.Interfaces
{
    public interface IJobRequisitionRepository : IGenericRepository<JobRequisition>
    {
    }

    public interface IJobInterviewRepository : IGenericRepository<JobInterview>
    {
    }

    public interface IJobOfferRepository : IGenericRepository<JobOffer>
    {
    }
}
