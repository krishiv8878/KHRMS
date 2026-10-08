using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure.Repositories
{
    public class JobRequisitionRepository : GenericRepository<JobRequisition>, IJobRequisitionRepository
    {
        public JobRequisitionRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }

    public class JobInterviewRepository : GenericRepository<JobInterview>, IJobInterviewRepository
    {
        public JobInterviewRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }

    public class JobOfferRepository : GenericRepository<JobOffer>, IJobOfferRepository
    {
        public JobOfferRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }
}
