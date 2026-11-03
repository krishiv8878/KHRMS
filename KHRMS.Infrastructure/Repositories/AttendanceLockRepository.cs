using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure.Repositories
{
    public class AttendanceLockRepository : GenericRepository<AttendanceLock>, IAttendanceLockRepository
    {
        public AttendanceLockRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }
}
