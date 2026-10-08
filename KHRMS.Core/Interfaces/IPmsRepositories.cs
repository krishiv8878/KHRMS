using KHRMS.Core.Models;

namespace KHRMS.Core.Interfaces
{
    public interface IPmsGoalRepository : IGenericRepository<PmsGoal>
    {
    }

    public interface IPmsKeyResultRepository : IGenericRepository<PmsKeyResult>
    {
    }

    public interface IPmsGoalCheckInRepository : IGenericRepository<PmsGoalCheckIn>
    {
    }

    public interface IPmsReviewCycleRepository : IGenericRepository<PmsReviewCycle>
    {
    }

    public interface IPmsAppraisalRepository : IGenericRepository<PmsAppraisal>
    {
    }

    public interface IPmsFeedbackRepository : IGenericRepository<PmsFeedback>
    {
    }

    public interface IPmsOneOnOneRepository : IGenericRepository<PmsOneOnOne>
    {
    }

    public interface IPmsPipRepository : IGenericRepository<PmsPip>
    {
    }
}
