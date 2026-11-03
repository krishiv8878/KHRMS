using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure.Repositories
{
    public class LeaveEncashmentRequestRepository : GenericRepository<LeaveEncashmentRequest>, ILeaveEncashmentRequestRepository
    {
        public LeaveEncashmentRequestRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }
}
