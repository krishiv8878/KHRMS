using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure.Repositories
{
    public class DailyAttendanceRecordRepository : GenericRepository<DailyAttendanceRecord>, IDailyAttendanceRecordRepository
    {
        public DailyAttendanceRecordRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }
}
