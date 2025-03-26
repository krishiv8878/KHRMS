using KHRMS.Core;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure
{
    public class EmailTemplateTypeMasterRepository : GenericRepository<EmailTemplateTypeMaster>, IEmailTemplateTypeMasterRepository
    {
        public EmailTemplateTypeMasterRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }

    }
}