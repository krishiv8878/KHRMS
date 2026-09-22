using KHRMS.Core;
using KHRMS.Core.Interfaces;

namespace KHRMS.Infrastructure.Repositories
{
    public class AssetRequestRepository : GenericRepository<AssetRequest>, IAssetRequestRepository
    {
        public AssetRequestRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }
}
