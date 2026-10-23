using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace KHRMS.Infrastructure.Repositories
{
    public class EmployeeDailyWorkModeRepository : GenericRepository<EmployeeDailyWorkMode>, IEmployeeDailyWorkModeRepository
    {
        private readonly KHRMSContextClass _context;

        public EmployeeDailyWorkModeRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<EmployeeDailyWorkMode?> GetByEmployeeAndDateAsync(long employeeId, DateOnly workDate)
        {
            return await _context.EmployeeDailyWorkModes
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.WorkDate == workDate);
        }

        public async Task<IEnumerable<EmployeeDailyWorkMode>> GetByEmployeeAndDateRangeAsync(long employeeId, DateOnly startDate, DateOnly endDate)
        {
            return await _context.EmployeeDailyWorkModes
                .Where(x => x.EmployeeId == employeeId && x.WorkDate >= startDate && x.WorkDate <= endDate)
                .OrderBy(x => x.WorkDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmployeeDailyWorkMode>> GetAllForDateAsync(DateOnly workDate)
        {
            return await _context.EmployeeDailyWorkModes
                .Where(x => x.WorkDate == workDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmployeeDailyWorkMode>> GetAllForDateRangeAsync(DateOnly startDate, DateOnly endDate)
        {
            return await _context.EmployeeDailyWorkModes
                .Where(x => x.WorkDate >= startDate && x.WorkDate <= endDate)
                .OrderBy(x => x.WorkDate)
                .ToListAsync();
        }
    }
}
