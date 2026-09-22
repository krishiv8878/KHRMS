using KHRMS.Core.Models;

namespace KHRMS.Services.Interfaces
{
    public interface IEmailTriggerEventService
    {
        Task<IEnumerable<EmailTriggerEvent>> GetAllAsync();
        Task<EmailTriggerEvent?> GetByEventCodeAsync(string eventCode);
        Task<bool> UpdateBindingAsync(long id, long? activeTemplateId, bool isEnabled, string? defaultSubject = null);
        Task<EmailTemplatesMaster?> GetActiveTemplateForEventAsync(string eventCodeOrLegacyName);
        Task<bool> SeedDefaultEventsAsync();
    }
}
