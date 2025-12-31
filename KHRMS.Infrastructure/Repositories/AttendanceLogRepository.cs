
using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure.Repositories
{
    public class AttendanceLogRepository : GenericRepository<AttendanceLog>, IAttendanceLogRepository
    {
        public AttendanceLogRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }
}
