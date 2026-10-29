using KHRMS.Core.Models;

namespace KHRMS.Core.Interfaces
{
    public interface IEmployeeDailyWorkModeRepository : IGenericRepository<EmployeeDailyWorkMode>
    {
        Task<EmployeeDailyWorkMode?> GetByEmployeeAndDateAsync(long employeeId, DateOnly workDate);
        Task<IEnumerable<EmployeeDailyWorkMode>> GetByEmployeeAndDateRangeAsync(long employeeId, DateOnly startDate, DateOnly endDate);
        Task<IEnumerable<EmployeeDailyWorkMode>> GetAllForDateAsync(DateOnly workDate);
        Task<IEnumerable<EmployeeDailyWorkMode>> GetAllForDateRangeAsync(DateOnly startDate, DateOnly endDate);
    }
}
