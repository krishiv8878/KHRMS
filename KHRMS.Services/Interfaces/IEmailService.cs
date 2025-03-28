
using KHRMS.Core.Models;

namespace KHRMS.Services
{
    public interface IEmailService
    {
        Task<Email> GetByEmailTemplatesIdAsync(long emailTemplateid);
        Task<IEnumerable<Email>> GetAllAsync();
        Task<Email> GetByIdAsync(long id);
        Task AddAsync(Email email);
        Task<bool> UpdateAsync(Email email);
        Task DeleteAsync(long id);
    }
}
