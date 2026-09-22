using KHRMS.Core;
using KHRMS.Core.Interfaces;

namespace KHRMS.Infrastructure.Repositories
{
    public class AssetRequestLogRepository : GenericRepository<AssetRequestLog>, IAssetRequestLogRepository
    {
        public AssetRequestLogRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }
}
