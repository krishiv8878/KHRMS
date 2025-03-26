using KHRMS.Core;
using KHRMS.Core.Models;

namespace KHRMS.Services
{
    public class EmailTemplateTypeService(IUnitOfWork unitOfWork) : IEmailTemplateTypeMasterService
    {
        public IUnitOfWork _unitOfWork = unitOfWork;
        public async Task AddAsync(EmailTemplateTypeMaster emailTemplateTypeMaster)
        {
            await _unitOfWork.EmailTemplateTypeMaster.Add(emailTemplateTypeMaster);
            var result = _unitOfWork.Save();
        }

        public async Task DeleteAsync(long id)
        {
            var emailTemplateType = await _unitOfWork.EmailTemplateTypeMaster.GetById(id);
            if (emailTemplateType != null)
            {
                _unitOfWork.EmailTemplateTypeMaster.Delete(emailTemplateType);
                _unitOfWork.Save();
            }
        }

        public async Task<IEnumerable<EmailTemplateTypeMaster>> GetAllAsync()
        {
            return await _unitOfWork.EmailTemplateTypeMaster.GetAll();
        }

        public async Task<EmailTemplateTypeMaster> GetByIdAsync(long id)
        {
            return await _unitOfWork.EmailTemplateTypeMaster.GetById(id);
        }

        public Task UpdateAsync(EmailTemplateTypeMaster emailTemplateTypeMaster)
        {
            _unitOfWork.EmailTemplateTypeMaster.Update(emailTemplateTypeMaster);
            var result = _unitOfWork.Save();
            return Task.CompletedTask;
        }
    }
}
