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
            if (string.IsNullOrWhiteSpace(document.Status))
            {
                document.Status = "Pending";
            }
            await _unitOfWork.EmployeementDocument.Add(document);
            _unitOfWork.Save();
        }
      
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

        public async Task<bool> ApproveOrRejectAsync(long id, string status, string? rejectionReason, long actionBy)
        {
            if (id <= 0) return false;

            var document = await _unitOfWork.EmployeementDocument.GetById(id);
            if (document == null || document.IsDeleted) return false;

            var normalizedStatus = status.Equals("Rejected", StringComparison.OrdinalIgnoreCase) ? "Rejected" : "Approved";
            document.Status = normalizedStatus;
            document.RejectionReason = normalizedStatus == "Rejected" ? rejectionReason : null;
            document.ActionBy = actionBy;
            document.ActionDate = DateTime.UtcNow;
            document.UpdatedBy = (int)actionBy;
            document.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.EmployeementDocument.Update(document);
            var result = _unitOfWork.Save();
            return result > 0;
        }
    }
}
