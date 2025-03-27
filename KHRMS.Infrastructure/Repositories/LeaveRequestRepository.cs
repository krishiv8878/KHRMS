
using KHRMS.Core;

namespace KHRMS.Infrastructure
{
    public class LeaveRequestRepository : GenericRepository<LeaveRequest>, ILeaveRequestRepository
    {
        public LeaveRequestRepository(KHRMSContextClass dbContext) : base(dbContext)
        {

        }
    }
}
