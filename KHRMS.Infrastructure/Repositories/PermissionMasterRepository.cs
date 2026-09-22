using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure.Repositories
{
    public class PermissionMasterRepository : GenericRepository<PermissionMaster>, IPermissionMasterRepository
    {
        public PermissionMasterRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }
}
