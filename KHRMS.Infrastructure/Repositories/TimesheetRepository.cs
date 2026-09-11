using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure.Repositories
{
    public class TimesheetRepository : GenericRepository<Timesheet>, ITimesheetRepository
    {
        public TimesheetRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }
}
