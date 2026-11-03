using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure.Repositories
{
    public class LopConversionRequestRepository : GenericRepository<LopConversionRequest>, ILopConversionRequestRepository
    {
        public LopConversionRequestRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }
}
