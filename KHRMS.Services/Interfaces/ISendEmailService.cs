
using KHRMS.Core.Models;

namespace KHRMS.Services
{
    public interface ISendEmailService
    {
        Task<bool> SendEmailAsync(Email email);
        Task<bool> SendTemplateEmailAsync(string toEmail, string subject ,Dictionary<string, string> placeholders, string templateType);
    }
}
