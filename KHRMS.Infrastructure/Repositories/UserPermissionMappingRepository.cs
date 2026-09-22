using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure.Repositories
{
    public class UserPermissionMappingRepository : GenericRepository<UserPermissionMapping>, IUserPermissionMappingRepository
    {
        public UserPermissionMappingRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }
}
