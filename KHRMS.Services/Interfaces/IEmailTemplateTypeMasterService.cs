
using KHRMS.Core.Models;

namespace KHRMS.Services
{
    public interface IEmailTemplateTypeMasterService
    {
        Task<IEnumerable<EmailTemplateTypeMaster>> GetAllAsync();
        Task<EmailTemplateTypeMaster> GetByIdAsync(long id);
        Task AddAsync(EmailTemplateTypeMaster emailTemplateTypeMaster);

        Task UpdateAsync(EmailTemplateTypeMaster emailTemplateTypeMaster);
        Task DeleteAsync(long id);
    }
}
