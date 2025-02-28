using KHRMS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KHRMS.Services
{
    public interface IDocumentMasterService
    {
        Task<IEnumerable<DocumentMaster>> GetAllAsync();
        Task<DocumentMaster> GetByIdAsync(long id);
        Task AddAsync(DocumentMaster document);
        Task UpdateAsync(DocumentMaster document);
        Task DeleteAsync(long id);
    }
}
