using KHRMS.Core;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure
{
    public class EmailRepository : GenericRepository<Email>, IEmailRepository
    {
        public EmailRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }

    }
}
