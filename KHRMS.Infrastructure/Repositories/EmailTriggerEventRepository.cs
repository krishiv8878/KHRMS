using KHRMS.Core;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure
{
    public class EmailTriggerEventRepository : GenericRepository<EmailTriggerEvent>, IEmailTriggerEventRepository
    {
        public EmailTriggerEventRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }
    }
}
