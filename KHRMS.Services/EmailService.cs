using KHRMS.Core;
using KHRMS.Core.Models;

namespace KHRMS.Services
{
    public class EmailService(IUnitOfWork unitOfWork,ISendEmailService sendEmailService) : IEmailService
    {
        public IUnitOfWork _unitOfWork = unitOfWork;
        public ISendEmailService _SendEmailService = sendEmailService;
        public async Task AddAsync(Email email)
        {
            var emails = new Email
            {
                Id = email.Id,
                EmailTemplateId = email.EmailTemplateId,
                EmailBody = email.EmailBody,
                EmailSubject = email.EmailSubject,
                FromEmail = email.FromEmail,
                ToEmail = email.ToEmail,
                IsDelivered = email.IsDelivered,
                CreatedDate = DateTime.Now,
                CreatedBy = email.CreatedBy,
                IsActive = email.IsActive,
            };
            await _unitOfWork.Email.Add(emails);
            var result = _unitOfWork.Save();
        }

        public async Task DeleteAsync(long id)
        {
            var emails = await _unitOfWork.Email.GetById(id);
            if (emails != null)
            {
                _unitOfWork.Email.Delete(emails);
                _unitOfWork.Save();
            }
        }

        public async Task<IEnumerable<Email>> GetAllAsync()
        {
            return await _unitOfWork.Email.GetAll();
        }

        public async Task<Email> GetByEmailTemplatesIdAsync(long emailTemplateid)
        {
            return await _unitOfWork.Email.GetById(emailTemplateid);
        }

        public async Task<Email> GetByIdAsync(long id)
        {
            return await _unitOfWork.Email.GetById(id);
        }

        public async Task<bool> UpdateAsync(Email email)
        {
            var emails = await _unitOfWork.Email.GetById(email.Id);
            if (emails == null)
            {
                return false;
            }
            emails.UpdatedDate = DateTime.Now;
            emails.EmailTemplateId = email.EmailTemplateId;
            emails.EmailBody = email.EmailBody;
            emails.EmailSubject = email.EmailSubject;
            emails.FromEmail = email.FromEmail;
            emails.ToEmail = email.ToEmail;
            emails.IsActive = email.IsActive;
            emails.IsDelivered = email.IsDelivered;
            emails.UpdatedBy = email.UpdatedBy;
            _unitOfWork.Email.Update(email);
            var result = _unitOfWork.Save();
            return true;
        }
    }
}
