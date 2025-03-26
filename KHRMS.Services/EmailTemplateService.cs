using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Infrastructure;

namespace KHRMS.Services
{
    public class EmailTemplateService(IUnitOfWork unitOfWork) : IEmailTemplateService
    {
        public IUnitOfWork _unitOfWork = unitOfWork;
        public async Task AddAsync(EmailTemplatesMaster emailTemplatesMaster)
        {
            var emailTemplets = new EmailTemplatesMaster
            {
                Id = emailTemplatesMaster.Id,
                EmailTemplateTypeId = emailTemplatesMaster.EmailTemplateTypeId,
                TemplateHtml = emailTemplatesMaster.TemplateHtml,
                CreatedDate = DateTime.Now,
                CreatedBy = emailTemplatesMaster.CreatedBy,
                IsActive = emailTemplatesMaster.IsActive,
                IsDeleted = emailTemplatesMaster.IsDeleted,
            };
            await _unitOfWork.EmailTemplateMaster.Add(emailTemplets);
            var result = _unitOfWork.Save();
        }

        public async Task DeleteAsync(long id)
        {
            var emailTemplate = await _unitOfWork.EmailTemplateMaster.GetById(id);
            if (emailTemplate != null)
            {
                _unitOfWork.EmailTemplateMaster.Delete(emailTemplate);
                _unitOfWork.Save();
            }
        }

        public async Task<IEnumerable<EmailTemplatesMaster>> GetAllAsync()
        {
            return await _unitOfWork.EmailTemplateMaster.GetAll();
        }

        public async Task<EmailTemplatesMaster> GetByEmailTemplateTypeIdAsync(long templateTypeid)
        {
            return await _unitOfWork.EmailTemplateMaster.GetById(templateTypeid);
        }

        public async Task<EmailTemplatesMaster> GetByIdAsync(long id)
        {
            return await _unitOfWork.EmailTemplateMaster.GetById(id);
        }

        public async Task<bool> UpdateAsync(EmailTemplatesMaster emailTemplatesMaster)
        {
            var emailtemplates = await _unitOfWork.EmailTemplateMaster.GetById(emailTemplatesMaster.Id);
            if (emailtemplates == null)
            {
                return false;
            }
            emailtemplates.UpdatedDate = DateTime.Now;
            emailtemplates.TemplateHtml = emailTemplatesMaster.TemplateHtml;
            emailtemplates.EmailTemplateTypeId = emailTemplatesMaster.EmailTemplateTypeId;
            emailtemplates.IsActive = emailTemplatesMaster.IsActive;
            emailtemplates.IsDeleted = emailTemplatesMaster.IsDeleted;
            emailtemplates.UpdatedBy = emailTemplatesMaster.UpdatedBy;
            _unitOfWork.EmailTemplateMaster.Update(emailtemplates);
            var result = _unitOfWork.Save();
            return true;
        }
    }
}
