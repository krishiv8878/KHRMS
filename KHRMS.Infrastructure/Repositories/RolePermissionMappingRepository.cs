using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure.Repositories
{
    public class RolePermissionMappingRepository : GenericRepository<RolePermissionMapping>, IRolePermissionMappingRepository
    {
        public RolePermissionMappingRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }
}
