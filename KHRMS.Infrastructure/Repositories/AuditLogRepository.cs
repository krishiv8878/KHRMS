using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure.Repositories
{
    public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(KHRMSContextClass context) : base(context) { }
    }
}
