using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure.Repositories
{
    public class PmsGoalRepository : GenericRepository<PmsGoal>, IPmsGoalRepository
    {
        public PmsGoalRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }

    public class PmsKeyResultRepository : GenericRepository<PmsKeyResult>, IPmsKeyResultRepository
    {
        public PmsKeyResultRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }

    public class PmsGoalCheckInRepository : GenericRepository<PmsGoalCheckIn>, IPmsGoalCheckInRepository
    {
        public PmsGoalCheckInRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }

    public class PmsReviewCycleRepository : GenericRepository<PmsReviewCycle>, IPmsReviewCycleRepository
    {
        public PmsReviewCycleRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }

    public class PmsAppraisalRepository : GenericRepository<PmsAppraisal>, IPmsAppraisalRepository
    {
        public PmsAppraisalRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }

    public class PmsFeedbackRepository : GenericRepository<PmsFeedback>, IPmsFeedbackRepository
    {
        public PmsFeedbackRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }

    public class PmsOneOnOneRepository : GenericRepository<PmsOneOnOne>, IPmsOneOnOneRepository
    {
        public PmsOneOnOneRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }

    public class PmsPipRepository : GenericRepository<PmsPip>, IPmsPipRepository
    {
        public PmsPipRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }
}
