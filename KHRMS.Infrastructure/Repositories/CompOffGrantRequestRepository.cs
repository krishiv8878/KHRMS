using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure.Repositories
{
    public class CompOffGrantRequestRepository : GenericRepository<CompOffGrantRequest>, ICompOffGrantRequestRepository
    {
        public CompOffGrantRequestRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }
}
