using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure.Repositories
{
    public class LeaveLedgerRepository : GenericRepository<LeaveLedger>, ILeaveLedgerRepository
    {
        public LeaveLedgerRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }
}
