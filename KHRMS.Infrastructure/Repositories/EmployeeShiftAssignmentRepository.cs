using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure.Repositories
{
    public class EmployeeShiftAssignmentRepository : GenericRepository<EmployeeShiftAssignment>, IEmployeeShiftAssignmentRepository
    {
        public EmployeeShiftAssignmentRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }
}
