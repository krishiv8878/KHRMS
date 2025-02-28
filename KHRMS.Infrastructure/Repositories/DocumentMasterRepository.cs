using KHRMS.Core;

namespace KHRMS.Infrastructure
{
    public class DocumentMasterRepository : GenericRepository<DocumentMaster>, IDocumentMasterRepository
    {
        public DocumentMasterRepository(KHRMSContextClass dbContext) : base(dbContext)
        {

        }
    }
}

