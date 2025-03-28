using KHRMS.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace KHRMS.Services
{
    public class EmployeeDocumentService : IEmployeeDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;
        public readonly string _uploadFolder;

        public EmployeeDocumentService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _uploadFolder = configuration["FileSettings:UploadFolder"] ?? "uploads";
        }

        public async Task<IEnumerable<EmployeeDocumentInfo>> GetAllAsync()
        {
            return await _unitOfWork.EmployeementDocument.GetAll();
        }

        public async Task<EmployeeDocumentInfo> GetByIdAsync(long id)
        {
            return await _unitOfWork.EmployeementDocument.GetById(id);
        }

        public async Task AddAsync(EmployeeDocumentInfo document)
        {
            await _unitOfWork.EmployeementDocument.Add(document);
            _unitOfWork.Save();
        }
      
        //public async Task DeleteAsync(long id)
        //{
        //    var document = await _unitOfWork.EmployeementDocument.GetById(id);
        //    if (document != null)
        //    {
        //        _unitOfWork.EmployeementDocument.Delete(document);
        //        _unitOfWork.Save();
        //    }
        //}
        public async Task<bool> DeleteAsync(long id)
        {
            if (id > 0)
            {
                var document = await _unitOfWork.EmployeementDocument.GetById(id);
                if (document != null)
                {
                    document.IsDeleted = true;
                    document.IsActive = false;

                    _unitOfWork.EmployeementDocument.Update(document);
                    var result = _unitOfWork.Save();

                    return result > 0;
                }
            }
            return false;
        }

    }
}
