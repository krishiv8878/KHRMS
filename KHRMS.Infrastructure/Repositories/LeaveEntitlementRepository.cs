using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure.Repositories
{
    public class LeaveEntitlementRepository : GenericRepository<LeaveEntitlement>, ILeaveEntitlementRepository
    {
        public LeaveEntitlementRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }
}
