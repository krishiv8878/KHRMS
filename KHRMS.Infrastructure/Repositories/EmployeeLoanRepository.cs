using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure.Repositories
{
    public class EmployeeLoanRepository : GenericRepository<EmployeeLoan>, IEmployeeLoanRepository
    {
        public EmployeeLoanRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }
}
