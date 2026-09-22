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
            var docs = (await _unitOfWork.EmployeementDocument.GetAll()).ToList();
            var empIds = docs.Select(d => d.EmployeeId).Union(docs.Select(d => d.UploadedBy)).Where(id => id > 0).Distinct().ToList();
            if (empIds.Count != 0)
            {
                var employees = (await _unitOfWork.Employees.GetAll())
                    .Where(e => empIds.Contains(e.Id))
                    .ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}".Trim());

                foreach (var doc in docs)
                {
                    if (employees.TryGetValue(doc.EmployeeId, out var name) && !string.IsNullOrWhiteSpace(name))
                    {
                        doc.EmployeeName = name;
                    }
                    else if (employees.TryGetValue(doc.UploadedBy, out var upName) && !string.IsNullOrWhiteSpace(upName))
                    {
                        doc.EmployeeName = upName;
                    }
                    else
                    {
                        doc.EmployeeName = $"Employee #{doc.EmployeeId}";
                    }

                    if (employees.TryGetValue(doc.UploadedBy, out var uName))
                    {
                        doc.UploadedByName = uName;
                    }
                }
            }
            return docs;
        }

        public async Task<EmployeeDocumentInfo> GetByIdAsync(long id)
        {
            var doc = await _unitOfWork.EmployeementDocument.GetById(id);
            if (doc != null)
            {
                if (doc.EmployeeId > 0)
                {
                    var emp = await _unitOfWork.Employees.GetById(doc.EmployeeId);
                    if (emp != null)
                    {
                        doc.EmployeeName = $"{emp.FirstName} {emp.LastName}".Trim();
                    }
                }
                if (doc.UploadedBy > 0)
                {
                    var upEmp = await _unitOfWork.Employees.GetById(doc.UploadedBy);
                    if (upEmp != null)
                    {
                        doc.UploadedByName = $"{upEmp.FirstName} {upEmp.LastName}".Trim();
                    }
                }
            }
            return doc;
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
