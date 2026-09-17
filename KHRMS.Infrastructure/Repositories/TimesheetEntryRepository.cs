using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure.Repositories
{
    public class TimesheetEntryRepository : GenericRepository<TimesheetEntry>, ITimesheetEntryRepository
    {
        public TimesheetEntryRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }
}
