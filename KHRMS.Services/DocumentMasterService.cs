using KHRMS.Core;
using KHRMS.Infrastructure;

namespace KHRMS.Services
{
    public class DocumentMasterServicee(IUnitOfWork unitOfWork) : IDocumentMasterService
    {
        public IUnitOfWork _unitOfWork = unitOfWork;

       
        public async Task<IEnumerable<DocumentMaster>> GetAllAsync()
        {
            var Getalldocuments = await _unitOfWork.DocumentMasters.GetAll();
            return Getalldocuments;
        }

        public async Task<DocumentMaster> GetByIdAsync(long id)
        {
            var Getalldocumentsbyid = await _unitOfWork.DocumentMasters.GetById(id);
            return Getalldocumentsbyid;
        }

        public async Task AddAsync(DocumentMaster document)
        {
            
            await _unitOfWork.DocumentMasters.Add(document);

        }

        public  Task UpdateAsync(DocumentMaster document)
        {
            _unitOfWork.DocumentMasters.Update(document);
            return Task.CompletedTask;
        }

        public  Task DeleteAsync(long id)
        {
            _unitOfWork.DocumentMasters.DeleteAsync(id);
            return Task.CompletedTask;
        }
    }
}


