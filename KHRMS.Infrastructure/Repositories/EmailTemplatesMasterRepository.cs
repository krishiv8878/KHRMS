using KHRMS.Core;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure
{
    public class EmailTemplatesMasterRepository : GenericRepository<EmailTemplatesMaster>, IEmailTemplateMasterRepository
    {
        public EmailTemplatesMasterRepository(KHRMSContextClass dbContext) : base(dbContext)
        {
        }

    }
}