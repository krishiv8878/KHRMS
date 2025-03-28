
using KHRMS.Core.Models;

namespace KHRMS.Services
{
    public interface IEmailTemplateService
    {
        Task<EmailTemplatesMaster> GetByEmailTemplateTypeIdAsync(long templateTypeid);
        Task<IEnumerable<EmailTemplatesMaster>> GetAllAsync();
        Task<EmailTemplatesMaster> GetByIdAsync(long id);
        Task AddAsync(EmailTemplatesMaster emailTemplatesMaster);

        Task<bool> UpdateAsync(EmailTemplatesMaster emailTemplatesMaster);
        Task DeleteAsync(long id);
    }
}
